using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Data.Entities;
using JustCSharp.Database.MongoDB.Attribute;
using JustCSharp.Database.MongoDB.Model;
using JustCSharp.Uow.UnitOfWork;
using JustCSharp.Utility.Extensions;
using JustCSharp.Utility.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace JustCSharp.Database.MongoDB.Context
{
    public class MongoDbContext : IMongoDbContext
    {
        #region Properties

        protected readonly MongoDbContextOptions _dbContextOptions;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMongoEntityModelCache _mongoEntityModelCache;

        protected bool _isConnected;
        protected string _databaseName;
        protected MongoUrl _mongoUrl;
        protected Dictionary<Type, IMongoEntityModel> _entityModels;
        protected IMongoClient _client;
        protected IMongoDatabase _database;
        protected IClientSessionHandle _sessionHandle;

        public MongoDbContextOptions DbContextOptions => _dbContextOptions;
        public bool IsConnected => _isConnected;
        public string DatabaseName => _databaseName;
        public MongoUrl MongoUrl => _mongoUrl;
        public Dictionary<Type, IMongoEntityModel> EntityModels => _entityModels;
        public IMongoClient Client => _client;

        public IMongoDatabase Database => _database;

        public IClientSessionHandle SessionHandle => _sessionHandle;

        #endregion

        #region Constructors

        public MongoDbContext(MongoDbContextOptions dbContextOptions, IUnitOfWork unitOfWork,
            IMongoEntityModelCache mongoEntityModelCache)
        {
            _dbContextOptions = dbContextOptions;
            _unitOfWork = unitOfWork;
            _mongoEntityModelCache = mongoEntityModelCache;

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
            if (_isConnected)
            {
                return;
            }

            var client = new MongoClient(_dbContextOptions.Settings);
            var database = client.GetDatabase(_databaseName);
            IClientSessionHandle session = null;

            if (_unitOfWork.IsTransactional)
            {
                var activeTransaction = _unitOfWork.FindTransaction(GetType()) as MongoDbTransaction;
                session = activeTransaction?.SessionHandle;

                if (session == null)
                {
                    session = Client.StartSession();

                    if (_dbContextOptions.Timeout.HasValue)
                    {
                        session.AdvanceOperationTime(new BsonTimestamp(_dbContextOptions.Timeout.Value));
                    }

                    session.StartTransaction();

                    _unitOfWork.GetOrAddTransaction(this.GetType(), () => new MongoDbTransaction(session));
                }
            }

            var modelCache = _mongoEntityModelCache.DbModelCache.GetOrDefault(this.GetType());
            if (modelCache == null)
            {
                InitializeDatabase(database, client, session);
                _mongoEntityModelCache.DbModelCache.TryAdd(this.GetType(), EntityModels);
            }
            else
            {
                InitializeDatabase(database, client, session, modelCache);
            }

            _unitOfWork.GetOrAddDatabase(this.GetType(), () => new MongoDbDatabase(this));

            _isConnected = true;
        }

        public async Task CheckStateAndConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_isConnected)
            {
                return;
            }


            var client = new MongoClient(_dbContextOptions.Settings);
            var database = client.GetDatabase(_databaseName);
            IClientSessionHandle session = null;

            if (_unitOfWork.IsTransactional)
            {
                var activeTransaction = _unitOfWork.FindTransaction(GetType()) as MongoDbTransaction;
                session = activeTransaction?.SessionHandle;

                if (session == null)
                {
                    session = await Client.StartSessionAsync(cancellationToken: cancellationToken);

                    if (_dbContextOptions.Timeout.HasValue)
                    {
                        session.AdvanceOperationTime(new BsonTimestamp(_dbContextOptions.Timeout.Value));
                    }

                    session.StartTransaction();

                    _unitOfWork.GetOrAddTransaction(this.GetType(), () => new MongoDbTransaction(session));
                }
            }

            var modelCache = _mongoEntityModelCache.DbModelCache.GetOrDefault(this.GetType());
            if (modelCache == null)
            {
                InitializeDatabase(database, client, session);
                _mongoEntityModelCache.DbModelCache.TryAdd(this.GetType(), EntityModels);
            }
            else
            {
                InitializeDatabase(database, client, session, modelCache);
            }

            _unitOfWork.GetOrAddDatabase(this.GetType(), () => new MongoDbDatabase(this));

            _isConnected = true;
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

            _entityModels = entityModels;
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
            _database = database;
            _client = client;
            _sessionHandle = sessionHandle;
            _entityModels = entityModels;
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
            _mongoUrl = new MongoUrl(_dbContextOptions.ConnectionString);
            _databaseName = _mongoUrl.DatabaseName;
            if (string.IsNullOrWhiteSpace(_databaseName))
            {
                _databaseName = _dbContextOptions.ConnectionStringName;
            }
        }

        #endregion
    }
}