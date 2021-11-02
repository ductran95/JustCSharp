using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.DependencyInjection;
using JustCSharp.Core.Logging.Extensions;
using JustCSharp.Data.Entities;
using JustCSharp.Database.MongoDB.Attribute;
using JustCSharp.Database.MongoDB.Model;
using JustCSharp.Uow.UnitOfWork;
using JustCSharp.Utility.Extensions;
using JustCSharp.Utility.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace JustCSharp.Database.MongoDB.Context
{
    public class MongoDbContext : IMongoDbContext
    {
        #region Properties

        protected readonly ILazyServiceProvider _serviceProvider;
        protected readonly MongoDbContextOptions _dbContextOptions;

        public bool IsConnected { get; protected set; }
        public string DatabaseName { get; protected set; }
        public MongoUrl MongoUrl { get; protected set; }
        public Dictionary<Type, IMongoEntityModel> EntityModels { get; protected set; }
        public MongoDbContextOptions DbContextOptions { get; protected set; }
        public IMongoClient Client { get; protected set; }
        public IMongoDatabase Database { get; protected set; }
        public IClientSessionHandle SessionHandle { get; protected set; }
        
        protected ILogger Logger => _serviceProvider.GetLogger(GetType());
        protected IUnitOfWork UnitOfWork => _serviceProvider.LazyGetService<IUnitOfWork>();
        protected IMongoEntityModelCache EntityModelCache => _serviceProvider.LazyGetService<IMongoEntityModelCache>();
        
        #endregion

        #region Constructors

        public MongoDbContext(ILazyServiceProvider serviceProvider, MongoDbContextOptions dbContextOptions)
        {
            _serviceProvider = serviceProvider;
            _dbContextOptions = dbContextOptions;

            ResolveConfig();
            
            if (!_dbContextOptions.LazyConnect)
            {
                CheckStateAndConnect();
            }
        }

        #endregion

        #region Functions

        public virtual IMongoCollection<T> Collection<T>()
        {
            return Database.GetCollection<T>(GetCollectionName<T>());
        }

        public void CheckStateAndConnect()
        {
            if (IsConnected)
            {
                return;
            }

            var client = new MongoClient(_dbContextOptions.Settings);
            var database = client.GetDatabase(DatabaseName);
            IClientSessionHandle session = null;

            var isTransactional = UnitOfWork?.IsTransactional ?? false;

            if (isTransactional)
            {
                var activeTransaction = UnitOfWork.FindTransaction(GetType()) as MongoDbTransaction;
                session = activeTransaction?.SessionHandle;

                if (session == null)
                {
                    session = Client.StartSession();

                    if (_dbContextOptions.Timeout.HasValue)
                    {
                        session.AdvanceOperationTime(new BsonTimestamp(_dbContextOptions.Timeout.Value));
                    }

                    session.StartTransaction();

                    UnitOfWork.GetOrAddTransaction(this.GetType(), () => new MongoDbTransaction(session));
                }
            }

            var modelCache = EntityModelCache.DbModelCache.GetOrDefault(this.GetType());
            if (modelCache == null)
            {
                InitializeDatabase(database, client, session);
                EntityModelCache.DbModelCache.TryAdd(this.GetType(), EntityModels);
            }
            else
            {
                InitializeDatabase(database, client, session, modelCache);
            }

            UnitOfWork?.GetOrAddDatabase(this.GetType(), () => new MongoDbDatabase(this));

            IsConnected = true;
        }

        public async Task CheckStateAndConnectAsync(CancellationToken cancellationToken = default)
        {
            if (IsConnected)
            {
                return;
            }

            var client = new MongoClient(_dbContextOptions.Settings);
            var database = client.GetDatabase(DatabaseName);
            IClientSessionHandle session = null;

            var isTransactional = UnitOfWork?.IsTransactional ?? false;

            if (isTransactional)
            {
                var activeTransaction = UnitOfWork.FindTransaction(GetType()) as MongoDbTransaction;
                session = activeTransaction?.SessionHandle;

                if (session == null)
                {
                    session = await Client.StartSessionAsync(cancellationToken: cancellationToken);

                    if (_dbContextOptions.Timeout.HasValue)
                    {
                        session.AdvanceOperationTime(new BsonTimestamp(_dbContextOptions.Timeout.Value));
                    }

                    session.StartTransaction();

                    UnitOfWork.GetOrAddTransaction(this.GetType(), () => new MongoDbTransaction(session));
                }
            }

            var modelCache = EntityModelCache.DbModelCache.GetOrDefault(this.GetType());
            if (modelCache == null)
            {
                InitializeDatabase(database, client, session);
                EntityModelCache.DbModelCache.TryAdd(this.GetType(), EntityModels);
            }
            else
            {
                InitializeDatabase(database, client, session, modelCache);
            }

            UnitOfWork?.GetOrAddDatabase(this.GetType(), () => new MongoDbDatabase(this));

            IsConnected = true;
        }

        protected virtual void InitializeCollections()
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
                var collectionAttribute = collectionProperty.GetCustomAttributes().OfType<MongoCollectionAttribute>()
                    .FirstOrDefault();

                modelBuilder.Entity(entityType,
                    b => { b.CollectionName = collectionAttribute?.CollectionName ?? collectionProperty.Name; });
            }

            // Invoke CreateModel
            CreateModel(modelBuilder);

            // Build Model
            var entityModels = modelBuilder.GetEntities()
                .ToDictionary(x => x.EntityType, x => x);

            foreach (var entityModel in entityModels.Values)
            {
                var map = entityModel.BsonMap;
                if (!BsonClassMap.IsClassMapRegistered(map.ClassType))
                {
                    BsonClassMap.RegisterClassMap(map);
                }

                if (!string.IsNullOrEmpty(entityModel.CollectionName))
                {
                    CreateCollectionIfNotExists(entityModel.CollectionName);
                }
            }

            EntityModels = entityModels;
        }

        protected virtual void InitializeDatabase(IMongoDatabase database, IMongoClient client,
            IClientSessionHandle sessionHandle)
        {
            InitializeDatabase(database, client, sessionHandle, EntityModels);
            InitializeCollections();
        }

        protected virtual void InitializeDatabase(IMongoDatabase database, IMongoClient client,
            IClientSessionHandle sessionHandle, Dictionary<Type, IMongoEntityModel> entityModels)
        {
            Database = database;
            Client = client;
            SessionHandle = sessionHandle;
            EntityModels = entityModels;
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
            var model = EntityModels.GetOrDefault(typeof(TEntity));

            if (model == null)
            {
                throw new MongoDbException("Could not find a model for given entity type: " +
                                           typeof(TEntity).AssemblyQualifiedName);
            }

            return model;
        }

        protected void ResolveConfig()
        {
            MongoUrl = new MongoUrl(_dbContextOptions.ConnectionString);
            DatabaseName = MongoUrl.DatabaseName;
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                DatabaseName = _dbContextOptions.ConnectionStringName;
            }
        }

        #endregion
    }
}