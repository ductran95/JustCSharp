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
using JustCSharp.Uow;
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

        protected readonly string _id;
        protected readonly ILazyServiceProvider _serviceProvider;
        protected readonly MongoDbContextOptions _dbContextOptions;
        protected MongoDbTransaction _currentTransaction;

        public MongoDbContextOptions DbContextOptions => _dbContextOptions;

        private ILogger Logger => _serviceProvider.GetLogger(typeof(MongoDbContext));
        
        public bool IsConnected { get; protected set; }
        public string DatabaseName { get; protected set; }
        public MongoUrl MongoUrl { get; protected set; }
        public Dictionary<Type, IMongoEntityModel> EntityModels { get; protected set; }
        public IMongoClient Client { get; protected set; }
        public IMongoDatabase Database { get; protected set; }
        public IClientSessionHandle SessionHandle { get; protected set; }
        public ITransaction CurrentTransaction => _currentTransaction;
        protected IUnitOfWork UnitOfWork => _serviceProvider.LazyGetService<IUnitOfWork>();
        protected IMongoEntityModelCache EntityModelCache => _serviceProvider.LazyGetService<IMongoEntityModelCache>();

        #endregion

        #region Constructors

        public MongoDbContext(ILazyServiceProvider serviceProvider, MongoDbContextOptions dbContextOptions)
        {
            _id = Guid.NewGuid().ToString();
            _serviceProvider = serviceProvider;
            _dbContextOptions = dbContextOptions;

            ResolveConfig();
            AddToUow();

            if (!_dbContextOptions.LazyConnect)
            {
                CheckStateAndConnect();
            }
        }

        #endregion

        #region Public Functions

        #region IMongoDbContext

        public virtual IMongoCollection<T> Collection<T>()
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.Collection");
            
            return Database.GetCollection<T>(GetCollectionName<T>());
        }

        public void CheckStateAndConnect()
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.CheckStateAndConnect");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start CheckStateAndConnect");
            
            if (IsConnected)
            {
                return;
            }

            if (DbContextOptions.ConcurrentInit)
            {
                // Setup as normal
                var result = CheckStateAndConnectInternal();

                InitializeDatabaseWithCache(result.Client, result.Database, result.Session);
            }
            else
            {
                var sync = MongoConnectionSynchronization.GetForType(GetType());

                if (sync.IsInitConnection)
                {
                    Logger.LogTrace("Sync.IsInitConnection true, context: {id}", _id);

                    // Setup as normal
                    var result = CheckStateAndConnectInternal();

                    InitializeDatabaseWithCache(result.Client, result.Database, result.Session);
                }
                else
                {
                    // Use mutex to prevent InitializeDatabaseAsync multiple at same time
                    // So modelCache return null => multiple DbContext call InitializeDatabaseAsync
                    // => BsonClassMap.RegisterClassMap call Add to dictionary failed

                    Logger.LogTrace("Sync.IsInitConnection false, context: {id}", _id);
                    var result = CheckStateAndConnectInternal();

                    // Lock when InitializeDatabase
                    Logger.LogTrace("Wait for mutex, context: {id}", _id);
                    sync.Mutex.Wait();
                    Logger.LogTrace("Got mutex, context: {id}", _id);

                    try
                    {
                        InitializeDatabaseWithCache(result.Client, result.Database, result.Session);

                        sync.IsInitConnection = true;
                    }
                    finally
                    {
                        sync.Mutex.Release();
                    }
                }
            }
            
            IsConnected = true;
            
            stopwatch?.Stop();
            Logger.LogInformation("End CheckStateAndConnect, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        public async Task CheckStateAndConnectAsync(CancellationToken cancellationToken = default)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.CheckStateAndConnectAsync");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start CheckStateAndConnectAsync");
            
            if (IsConnected)
            {
                return;
            }

            if (DbContextOptions.ConcurrentInit)
            {
                // Setup as normal
                var result = await CheckStateAndConnectInternalAsync(cancellationToken);

                await InitializeDatabaseWithCacheAsync(result.Client, result.Database, result.Session,
                    cancellationToken);
            }
            else
            {
                var sync = MongoConnectionSynchronization.GetForType(GetType());

                if (sync.IsInitConnection)
                {
                    Logger.LogTrace("Sync.IsInitConnection true, context: {id}", _id);

                    // Setup as normal
                    var result = await CheckStateAndConnectInternalAsync(cancellationToken);

                    await InitializeDatabaseWithCacheAsync(result.Client, result.Database, result.Session,
                        cancellationToken);
                }
                else
                {
                    // Use mutex to prevent InitializeDatabaseAsync multiple at same time
                    // So modelCache return null => multiple DbContext call InitializeDatabaseAsync
                    // => BsonClassMap.RegisterClassMap call Add to dictionary failed

                    Logger.LogTrace("Sync.IsInitConnection false, context: {id}", _id);
                    var result = await CheckStateAndConnectInternalAsync(cancellationToken);

                    // Lock when InitializeDatabase
                    Logger.LogTrace("Wait for mutex, context: {id}", _id);
                    await sync.Mutex.WaitAsync(cancellationToken);
                    Logger.LogTrace("Got mutex, context: {id}", _id);

                    try
                    {
                        await InitializeDatabaseWithCacheAsync(result.Client, result.Database, result.Session,
                            cancellationToken);

                        sync.IsInitConnection = true;
                    }
                    finally
                    {
                        sync.Mutex.Release();
                    }
                }
            }
            
            IsConnected = true;
            
            stopwatch?.Stop();
            Logger.LogInformation("End CheckStateAndConnectAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        #endregion
        
        #region IDatabase
        
        #endregion
        
        #region ISupportTransaction
        
        public bool IsInTransaction()
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.IsInTransaction");
            
            return IsConnected && SessionHandle != null;
        }

        public Task<bool> IsInTransactionAsync(CancellationToken cancellationToken = default)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.IsInTransactionAsync");
            
            return Task.FromResult(IsConnected && SessionHandle != null);
        }

        public void BeginTransaction()
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.BeginTransaction");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start BeginTransaction");

            if (!IsConnected)
            {
                CheckStateAndConnect();
                return;
            }
            
            if (IsInTransaction())
            {
                Logger.LogWarning("Transaction already began");
                return;
            }
            
            var session = CreateSession(Client);
            if (_currentTransaction == null)
            {
                _currentTransaction = new MongoDbTransaction(session);
            }
            else
            {
                _currentTransaction.SessionHandle = session;
            }
            
            stopwatch?.Stop();
            Logger.LogInformation("End BeginTransaction, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.BeginTransactionAsync");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start BeginTransactionAsync");
            
            if (!IsConnected)
            {
                await CheckStateAndConnectAsync(cancellationToken);
                return;
            }
            
            if (await IsInTransactionAsync(cancellationToken))
            {
                Logger.LogWarning("Transaction already began");
                return;
            }
            
            var session = await CreateSessionAsync(Client, cancellationToken);
            if (_currentTransaction == null)
            {
                _currentTransaction = new MongoDbTransaction(session);
            }
            else
            {
                _currentTransaction.SessionHandle = session;
            }
            
            stopwatch?.Stop();
            Logger.LogInformation("End BeginTransactionAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        public void CommitTransaction()
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.CommitTransaction");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start CommitTransaction");
            
            if (!IsConnected)
            {
                Logger.LogWarning("Context has not connected to Database");
                return;
            }
            
            if (!IsInTransaction())
            {
                Logger.LogWarning("Transaction has not began");
                return;
            }
            
            SessionHandle.CommitTransaction();
            
            stopwatch?.Stop();
            Logger.LogInformation("End CommitTransaction, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.CommitTransactionAsync");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start CommitTransactionAsync");
            
            if (!IsConnected)
            {
                Logger.LogWarning("Context has not connected to Database");
                return;
            }
            
            if (!(await IsInTransactionAsync(cancellationToken)))
            {
                Logger.LogWarning("Transaction has not began");
                return;
            }
            
            await SessionHandle.CommitTransactionAsync(cancellationToken);
            
            stopwatch?.Stop();
            Logger.LogInformation("End CommitTransactionAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        public void RollbackTransaction()
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.RollbackTransaction");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start RollbackTransaction");
            
            if (!IsConnected)
            {
                Logger.LogWarning("Context has not connected to Database");
                return;
            }
            
            if (!IsInTransaction())
            {
                Logger.LogWarning("Transaction has not began");
                return;
            }
            
            SessionHandle.AbortTransaction();
            
            stopwatch?.Stop();
            Logger.LogInformation("End RollbackTransaction, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.RollbackTransactionAsync");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start RollbackTransactionAsync");
            
            if (!IsConnected)
            {
                Logger.LogWarning("Context has not connected to Database");
                return;
            }
            
            if (!(await IsInTransactionAsync(cancellationToken)))
            {
                Logger.LogWarning("Transaction has not began");
                return;
            }
            
            await SessionHandle.AbortTransactionAsync(cancellationToken);
            
            stopwatch?.Stop();
            Logger.LogInformation("End RollbackTransactionAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }
        
        #endregion

        #endregion
        
        #region Protected Functions
        
        protected virtual void InitializeCollections()
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.InitializeCollections");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start InitializeCollections");
            
            var entityModels = InitializeCollectionsInternal();

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
            
            stopwatch?.Stop();
            Logger.LogInformation("End InitializeCollections, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        protected virtual async Task InitializeCollectionsAsync(CancellationToken cancellationToken = default)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.InitializeCollectionsAsync");
            
            var stopwatch = Logger.StartStopwatch();
            Logger.LogInformation($"Start InitializeCollectionsAsync");
            
            var entityModels = InitializeCollectionsInternal();

            foreach (var entityModel in entityModels.Values)
            {
                var map = entityModel.BsonMap;
                if (!BsonClassMap.IsClassMapRegistered(map.ClassType))
                {
                    BsonClassMap.RegisterClassMap(map);
                }

                if (!string.IsNullOrEmpty(entityModel.CollectionName))
                {
                    await CreateCollectionIfNotExistsAsync(entityModel.CollectionName, cancellationToken);
                }
            }

            EntityModels = entityModels;
            
            stopwatch?.Stop();
            Logger.LogInformation("End InitializeCollectionsAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        protected virtual void InitializeDatabase(IMongoDatabase database, IMongoClient client,
            IClientSessionHandle sessionHandle)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.InitializeDatabase");
            
            InitializeDatabaseConfig(database, client, sessionHandle, EntityModels);
            InitializeCollections();
        }

        protected virtual async Task InitializeDatabaseAsync(IMongoDatabase database, IMongoClient client,
            IClientSessionHandle sessionHandle, CancellationToken cancellationToken = default)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.InitializeDatabaseAsync");
            
            InitializeDatabaseConfig(database, client, sessionHandle, EntityModels);
            await InitializeCollectionsAsync(cancellationToken);
        }

        protected virtual void InitializeDatabaseConfig(IMongoDatabase database, IMongoClient client,
            IClientSessionHandle sessionHandle, Dictionary<Type, IMongoEntityModel> entityModels)
        {
            using var activity = Trace.ActivitySource.StartActivity("MongoDbContext.InitializeDatabaseConfig");
            
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
                var stopwatch = Logger.StartStopwatch();
                Logger.LogInformation("Collection {collectionName} not existed, start CreateCollectionIfNotExists",
                    collectionName);
                Database.CreateCollection(collectionName);
                stopwatch?.Stop();
                Logger.LogInformation("End CreateCollectionIfNotExists, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
                stopwatch = null;
            }
        }

        protected virtual async Task CreateCollectionIfNotExistsAsync(string collectionName,
            CancellationToken cancellationToken = default)
        {
            var filter = new BsonDocument("name", collectionName);
            var options = new ListCollectionNamesOptions { Filter = filter };

            var result = await Database.ListCollectionNamesAsync(options, cancellationToken);

            if (!await result.AnyAsync(cancellationToken: cancellationToken))
            {
                var stopwatch = Logger.StartStopwatch();
                Logger.LogInformation("Collection {collectionName} not existed, start CreateCollectionIfNotExistsAsync",
                    collectionName);
                await Database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
                stopwatch?.Stop();
                Logger.LogInformation("End CreateCollectionIfNotExistsAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
                stopwatch = null;
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
        
        #endregion

        #region Private Functions
        
        private void ResolveConfig()
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start ResolveConfig");
            
            MongoUrl = new MongoUrl(_dbContextOptions.ConnectionString);
            DatabaseName = MongoUrl.DatabaseName;
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                DatabaseName = !string.IsNullOrWhiteSpace(_dbContextOptions.DatabaseName) ? _dbContextOptions.DatabaseName : _dbContextOptions.ConnectionStringName;
            }
            
            stopwatch?.Stop();
            Logger.LogTrace("End ResolveConfig, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }
        
        private void AddToUow()
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start AddToUow");
            
            if (UnitOfWork != null)
            {
                var existedDatabase = UnitOfWork.FindDatabase(GetType());
                if (existedDatabase != null)
                {
                    Logger.LogWarning("Current Unit Of Work has already had this DbContext");
                }
                else
                {
                    UnitOfWork.GetOrAddDatabase(GetType(), () => this);
                }
            }
            
            stopwatch?.Stop();
            Logger.LogTrace("End AddToUow, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }
        
        private Dictionary<Type, IMongoEntityModel> InitializeCollectionsInternal()
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start InitializeCollectionsInternal");

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

            stopwatch?.Stop();
            Logger.LogTrace("End InitializeCollectionsInternal, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;

            return entityModels;
        }
        
        private (MongoClient Client, IMongoDatabase Database, IClientSessionHandle Session)
            CheckStateAndConnectInternal()
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start CheckStateAndConnectInternal");

            var client = new MongoClient(_dbContextOptions.Settings);
            var database = client.GetDatabase(DatabaseName);
            IClientSessionHandle session = null;

            var isTransactional = UnitOfWork?.IsTransactional ?? false;

            if (isTransactional)
            {
                session = CreateSession(client);
                if (_currentTransaction == null)
                {
                    _currentTransaction = new MongoDbTransaction(session);
                }
                else
                {
                    _currentTransaction.SessionHandle = session;
                }
            }
            
            stopwatch?.Stop();
            Logger.LogTrace("End CheckStateAndConnectInternal, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;

            return (client, database, session);
        }

        private async Task<(MongoClient Client, IMongoDatabase Database, IClientSessionHandle Session)>
            CheckStateAndConnectInternalAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start CheckStateAndConnectInternalAsync");

            var client = new MongoClient(_dbContextOptions.Settings);
            var database = client.GetDatabase(DatabaseName);
            IClientSessionHandle session = null;

            var isTransactional = UnitOfWork?.IsTransactional ?? false;

            if (isTransactional)
            {
                session = await CreateSessionAsync(client, cancellationToken);
                if (_currentTransaction == null)
                {
                    _currentTransaction = new MongoDbTransaction(session);
                }
                else
                {
                    _currentTransaction.SessionHandle = session;
                }
            }

            stopwatch?.Stop();
            Logger.LogTrace("End CheckStateAndConnectInternalAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;

            return (client, database, session);
        }

        private void InitializeDatabaseWithCache(MongoClient client, IMongoDatabase database, IClientSessionHandle session)
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start InitializeDatabaseWithCache");
            
            var modelCache = EntityModelCache.DbModelCache.GetOrDefault(this.GetType());
            if (modelCache == null)
            {
                InitializeDatabase(database, client, session);
                EntityModelCache.DbModelCache.TryAdd(this.GetType(), EntityModels);
            }
            else
            {
                InitializeDatabaseConfig(database, client, session, modelCache);
            }
            
            stopwatch?.Stop();
            Logger.LogTrace("End InitializeDatabaseWithCache, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }
        
        private async Task InitializeDatabaseWithCacheAsync(MongoClient client, IMongoDatabase database, IClientSessionHandle session, CancellationToken cancellationToken = default)
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start InitializeDatabaseWithCacheAsync");
            
            var modelCache = EntityModelCache.DbModelCache.GetOrDefault(this.GetType());
            if (modelCache == null)
            {
                await InitializeDatabaseAsync(database, client, session,
                    cancellationToken);
                EntityModelCache.DbModelCache.TryAdd(this.GetType(), EntityModels);
            }
            else
            {
                InitializeDatabaseConfig(database, client, session, modelCache);
            }
            
            stopwatch?.Stop();
            Logger.LogTrace("End InitializeDatabaseWithCacheAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;
        }

        private IClientSessionHandle CreateSession(IMongoClient client)
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start CreateSession");
            
            var session = client.StartSession();

            if (_dbContextOptions.Timeout.HasValue)
            {
                session.AdvanceOperationTime(new BsonTimestamp(_dbContextOptions.Timeout.Value));
            }

            session.StartTransaction();
            
            stopwatch?.Stop();
            Logger.LogTrace("End CreateSession, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;

            return session;
        }
        
        private async Task<IClientSessionHandle> CreateSessionAsync(IMongoClient client, CancellationToken cancellationToken = default)
        {
            var stopwatch = Logger.StartStopwatch();
            Logger.LogTrace($"Start CreateSessionAsync");
            
            var session = await client.StartSessionAsync(cancellationToken: cancellationToken);

            if (_dbContextOptions.Timeout.HasValue)
            {
                session.AdvanceOperationTime(new BsonTimestamp(_dbContextOptions.Timeout.Value));
            }

            session.StartTransaction();
            
            stopwatch?.Stop();
            Logger.LogTrace("End CreateSessionAsync, elapsed time: {elapsedTime}", Logger.GetElapsedTime(stopwatch));
            stopwatch = null;

            return session;
        }
        #endregion
    }
}