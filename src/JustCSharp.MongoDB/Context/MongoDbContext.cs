using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using JustCSharp.Core.Utility.Extensions;
using JustCSharp.Core.Utility.Helpers;
using JustCSharp.Data;
using JustCSharp.MongoDB.Attribute;
using JustCSharp.MongoDB.Model;
using MongoDB.Driver;

namespace JustCSharp.MongoDB.Context
{
    public class MongoDbContext: IMongoDbContext
    {
        public ConcurrentDictionary<Type, MongoEntityModel> ModelCache { get; private set; }
        
        public IMongoClient Client { get; private set; }

        public IMongoDatabase Database { get; private set; }

        public IClientSessionHandle SessionHandle { get; private set; }

        public virtual void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle sessionHandle)
        {
            InitializeCollections();
            InitializeDatabase(database, client, sessionHandle, ModelCache);
        }
        
        public virtual void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle sessionHandle, ConcurrentDictionary<Type, MongoEntityModel> modelCache)
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

                ModelCache.GetOrAdd(entityType, () => new MongoEntityModel()
                {
                    EntityType = entityType,
                    CollectionName = collectionAttribute?.CollectionName ?? collectionProperty.Name
                });
            }
        }

        protected virtual string GetCollectionName<T>()
        {
            return GetEntityModel<T>().CollectionName;
        }

        protected virtual MongoEntityModel GetEntityModel<TEntity>()
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