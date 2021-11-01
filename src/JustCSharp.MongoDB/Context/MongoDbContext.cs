using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JustCSharp.Data.Entities;
using JustCSharp.MongoDB.Attribute;
using JustCSharp.MongoDB.Model;
using JustCSharp.Utility.Extensions;
using JustCSharp.Utility.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace JustCSharp.MongoDB.Context
{
    public class MongoDbContext: IMongoDbContext
    {
        public Dictionary<Type, IMongoEntityModel> ModelCache { get; private set; }
        
        public IMongoClient Client { get; private set; }

        public IMongoDatabase Database { get; private set; }

        public IClientSessionHandle SessionHandle { get; private set; }

        public virtual void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle sessionHandle)
        {
            InitializeCollections();
            InitializeDatabase(database, client, sessionHandle, ModelCache);
        }
        
        public virtual void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle sessionHandle, Dictionary<Type, IMongoEntityModel> modelCache)
        {
            Database = database;
            Client = client;
            SessionHandle = sessionHandle;
            ModelCache = modelCache;
        }

        public virtual IMongoCollection<T> Collection<T>()
        {
            return Database.GetCollection<T>(GetCollectionName<T>());
        }

        public virtual void InitializeCollections()
        {
            var modelBuilder = new MongoModelBuilder();
            
            // Invoke MongoCollectionAttribute
            var collectionProperties =
                from property in this.GetType().GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where
                    ReflectionHelper.IsAssignableToGenericType(property.PropertyType, typeof(IMongoCollection<>)) &&
                    typeof(IEntity).IsAssignableFrom(property.PropertyType.GenericTypeArguments[0])
                select property;

            foreach (var collectionProperty in collectionProperties)
            {
                var entityType = collectionProperty.PropertyType.GenericTypeArguments[0];
                var collectionAttribute = collectionProperty.GetCustomAttributes().OfType<MongoCollectionAttribute>().FirstOrDefault();

                modelBuilder.Entity(entityType, b =>
                {
                    b.CollectionName = collectionAttribute?.CollectionName ?? collectionProperty.Name;
                });
            }
            
            // Invoke CreateModel
            CreateModel(modelBuilder);
            
            // Build Model
            var entityModels = modelBuilder.GetEntities()
                .ToDictionary(x=>x.EntityType, x=>x);
            
            foreach (var entityModel in entityModels.Values)
            {
                var map = entityModel.BsonMap;
                if (!BsonClassMap.IsClassMapRegistered(map.ClassType))
                {
                    BsonClassMap.RegisterClassMap(map);
                }

                CreateCollectionIfNotExists(entityModel.CollectionName);
            }

            ModelCache = entityModels;
        }

        protected virtual void CreateModel(MongoModelBuilder modelBuilder)
        {
            
        }
        
        protected virtual void CreateCollectionIfNotExists(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var options = new ListCollectionNamesOptions { Filter = filter };

            if (!Database.ListCollectionNames(options).Any())
            {
                Database.CreateCollection(collectionName);
            }
        }

        protected virtual string GetCollectionName<T>()
        {
            return GetEntityModel<T>().CollectionName;
        }

        protected virtual IMongoEntityModel GetEntityModel<TEntity>()
        {
            var model = ModelCache.GetOrDefault(typeof(TEntity));

            if (model == null)
            {
                throw new MongoDbException("Could not find a model for given entity type: " + typeof(TEntity).AssemblyQualifiedName);
            }

            return model;
        }
    }
}