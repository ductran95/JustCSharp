using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        protected readonly string _id;
        protected readonly ILazyServiceProvider _serviceProvider;
        protected readonly MongoDbContextOptions _dbContextOptions;

        public MongoDbContextOptions DbContextOptions => _dbContextOptions;

        private ILogger Logger => _serviceProvider.GetLogger(typeof(MongoDbContext));
        
        public bool IsConnected { get; protected set; }
        public string DatabaseName { get; protected set; }
        public MongoUrl MongoUrl { get; protected set; }
        public Dictionary<Type, IMongoEntityModel> EntityModels { get; protected set; }
        public IMongoClient Client { get; protected set; }
        public IMongoDatabase Database { get; protected set; }
        public IClientSessionHandle SessionHandle { get; protected set; }
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
                    Logger.LogTrace("sync.IsInitConnection true, context: {id}", _id);

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
        }

        public async Task CheckStateAndConnectAsync(CancellationToken cancellationToken = default)
        {
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
                    Logger.LogTrace("sync.IsInitConnection true, context: {id}", _id);

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
        }

        #endregion
        
        #region Protected Functions
        
        protected virtual void InitializeCollections()
        {
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
        }

        protected virtual async Task InitializeCollectionsAsync(CancellationToken cancellationToken = default)
        {
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
        }

        protected virtual void InitializeDatabase(IMongoDatabase database, IMongoClient client,
            IClientSessionHandle sessionHandle)
        {
            InitializeDatabase(database, client, sessionHandle, EntityModels);
            InitializeCollections();
        }

        protected virtual async Task InitializeDatabaseAsync(IMongoDatabase database, IMongoClient client,
            IClientSessionHandle sessionHandle, CancellationToken cancellationToken = default)
        {
            InitializeDatabase(database, client, sessionHandle, EntityModels);
            await InitializeCollectionsAsync(cancellationToken);
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
            Stopwatch stopwatch = null;
            TimeSpan elapsedTime = TimeSpan.Zero;
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                stopwatch = Stopwatch.StartNew();
            }

            Logger.LogTrace("Start CreateCollectionIfNotExists with collection name {collectionName}", collectionName);

            var filter = new BsonDocument("name", collectionName);
            var options = new ListCollectionNamesOptions { Filter = filter };

            if (!Database.ListCollectionNames(options).Any())
            {
                stopwatch?.Stop();
                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    elapsedTime = elapsedTime.Add(stopwatch?.Elapsed ?? TimeSpan.Zero);
                }

                stopwatch?.Restart();

                Database.CreateCollection(collectionName);

                Logger.LogTrace("CreateCollection takes {time}", stopwatch?.Elapsed);
            }

            stopwatch?.Stop();
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                elapsedTime = elapsedTime.Add(stopwatch?.Elapsed ?? TimeSpan.Zero);
            }

            Logger.LogTrace("End CreateCollectionIfNotExists, elapsed time {time}", elapsedTime);
        }

        protected virtual async Task CreateCollectionIfNotExistsAsync(string collectionName,
            CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = null;
            TimeSpan elapsedTime = TimeSpan.Zero;
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                stopwatch = Stopwatch.StartNew();
            }

            Logger.LogTrace("Start CreateCollectionIfNotExistsAsync with collection name {collectionName}",
                collectionName);

            var filter = new BsonDocument("name", collectionName);
            var options = new ListCollectionNamesOptions { Filter = filter };

            var result = await Database.ListCollectionNamesAsync(options, cancellationToken);

            if (!await result.AnyAsync(cancellationToken: cancellationToken))
            {
                stopwatch?.Stop();
                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    elapsedTime = elapsedTime.Add(stopwatch?.Elapsed ?? TimeSpan.Zero);
                }

                stopwatch?.Restart();

                await Database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);

                Logger.LogTrace("CreateCollectionAsync takes {time}", stopwatch?.Elapsed);
            }

            stopwatch?.Stop();
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                elapsedTime = elapsedTime.Add(stopwatch?.Elapsed ?? TimeSpan.Zero);
            }

            Logger.LogTrace("End CreateCollectionIfNotExistsAsync, elapsed time {time}", elapsedTime);
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
            MongoUrl = new MongoUrl(_dbContextOptions.ConnectionString);
            DatabaseName = MongoUrl.DatabaseName;
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                DatabaseName = !string.IsNullOrWhiteSpace(_dbContextOptions.DatabaseName) ? _dbContextOptions.DatabaseName : _dbContextOptions.ConnectionStringName;
            }
        }
        
        private void AddToUow()
        {
            if (UnitOfWork != null)
            {
                var existedDatabase = UnitOfWork.FindDatabase(GetType());
                if (existedDatabase != null)
                {
                    Logger.LogWarning("Current Unit Of Work has already had this DbContext");
                }
                else
                {
                    UnitOfWork.GetOrAddDatabase(GetType(), () => new MongoDbDatabase(this));
                }
            }
        }
        
        private Dictionary<Type, IMongoEntityModel> InitializeCollectionsInternal()
        {
            Stopwatch stopwatch = null;
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                stopwatch = Stopwatch.StartNew();
            }

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
            Logger.LogTrace("End InitializeCollectionsAsync, elapsed time {time}", stopwatch?.Elapsed);

            return entityModels;
        }
        
        private (MongoClient Client, IMongoDatabase Database, IClientSessionHandle Session)
            CheckStateAndConnectInternal()
        {
            Stopwatch stopwatch = null;
            Logger.LogTrace($"Start CheckStateAndConnectInternal");

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
                    if (Logger.IsEnabled(LogLevel.Trace))
                    {
                        stopwatch = Stopwatch.StartNew();
                    }

                    Logger.LogTrace($"Start StartSession");

                    session = Client.StartSession();

                    stopwatch?.Stop();
                    Logger.LogTrace("End StartSession, elapsed time {elapsedTime}", stopwatch?.Elapsed);

                    if (_dbContextOptions.Timeout.HasValue)
                    {
                        session.AdvanceOperationTime(new BsonTimestamp(_dbContextOptions.Timeout.Value));
                    }

                    session.StartTransaction();

                    UnitOfWork.GetOrAddTransaction(this.GetType(), () => new MongoDbTransaction(session));
                }
            }

            stopwatch?.Stop();
            Logger.LogTrace("End CheckStateAndConnectInternal");

            return (client, database, session);
        }

        private async Task<(MongoClient Client, IMongoDatabase Database, IClientSessionHandle Session)>
            CheckStateAndConnectInternalAsync(CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = null;
            Logger.LogTrace($"Start CheckStateAndConnectInternalAsync");

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
                    if (Logger.IsEnabled(LogLevel.Trace))
                    {
                        stopwatch = Stopwatch.StartNew();
                    }

                    Logger.LogTrace($"Start StartSessionAsync");

                    session = await Client.StartSessionAsync(cancellationToken: cancellationToken);

                    stopwatch?.Stop();
                    Logger.LogTrace("End StartSessionAsync, elapsed time {elapsedTime}", stopwatch?.Elapsed);

                    if (_dbContextOptions.Timeout.HasValue)
                    {
                        session.AdvanceOperationTime(new BsonTimestamp(_dbContextOptions.Timeout.Value));
                    }

                    session.StartTransaction();

                    UnitOfWork.GetOrAddTransaction(this.GetType(), () => new MongoDbTransaction(session));
                }
            }

            stopwatch?.Stop();
            Logger.LogTrace("End CheckStateAndConnectInternalAsync");

            return (client, database, session);
        }

        private void InitializeDatabaseWithCache(MongoClient client, IMongoDatabase database, IClientSessionHandle session)
        {
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
        }
        
        private async Task InitializeDatabaseWithCacheAsync(MongoClient client, IMongoDatabase database, IClientSessionHandle session, CancellationToken cancellationToken = default)
        {
            var modelCache = EntityModelCache.DbModelCache.GetOrDefault(this.GetType());
            if (modelCache == null)
            {
                await InitializeDatabaseAsync(database, client, session,
                    cancellationToken);
                EntityModelCache.DbModelCache.TryAdd(this.GetType(), EntityModels);
            }
            else
            {
                InitializeDatabase(database, client, session, modelCache);
            }
        }

        #endregion
    }
}