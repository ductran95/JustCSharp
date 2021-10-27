using System;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.Utility.Extensions;
using JustCSharp.MongoDB.Model;
using JustCSharp.Uow.UnitOfWork;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace JustCSharp.MongoDB.Context
{
    public class UowMongoDbContextProvider: IMongoDbContextProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly MongoDbContextOptions _mongoDbContextOptions;
        
        public UowMongoDbContextProvider(IServiceProvider serviceProvider, IUnitOfWorkProvider unitOfWorkProvider, IOptions<MongoDbContextOptions> mongoDbContextOptions)
        {
            _serviceProvider = serviceProvider;
            _unitOfWorkProvider = unitOfWorkProvider;
            _mongoDbContextOptions = mongoDbContextOptions.Value;
        }
        
        public TDbContext GetDbContext<TDbContext>() where TDbContext: IMongoDbContext
        {
            var unitOfWork = _unitOfWorkProvider.GetUnitOfWork();
            if (unitOfWork == null)
            {
                throw new MongoDbException(
                    $"A {nameof(IMongoDatabase)} instance can only be created inside a unit of work!");
            }

            var targetDbContextType = typeof(TDbContext);
            var dbConfig = ResolveDbConfig(targetDbContextType);

            var mongoUrl = new MongoUrl(dbConfig.ConnectionString);
            var databaseName = mongoUrl.DatabaseName;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                databaseName = dbConfig.ConnectionStringName;
            }
            
            var database = unitOfWork.FindDatabase(targetDbContextType);
            if (database == null)
            {
                database = new MongoDbDatabase(
                    CreateDbContext<TDbContext>(
                        unitOfWork,
                        dbConfig,
                        databaseName
                    )
                );

                unitOfWork.AddDatabase(targetDbContextType, database);
            }

            return (TDbContext)((MongoDbDatabase) database).DbContext;
        }

        public async Task<TDbContext> GetDbContextAsync<TDbContext>(CancellationToken cancellationToken = default) where TDbContext: IMongoDbContext
        {
            var unitOfWork = _unitOfWorkProvider.GetUnitOfWork();
            if (unitOfWork == null)
            {
                throw new MongoDbException(
                    $"A {nameof(IMongoDatabase)} instance can only be created inside a unit of work!");
            }

            var targetDbContextType = typeof(TDbContext);
            var dbConfig = ResolveDbConfig(targetDbContextType);

            var mongoUrl = new MongoUrl(dbConfig.ConnectionString);
            var databaseName = mongoUrl.DatabaseName;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                databaseName = dbConfig.ConnectionStringName;
            }
            
            var database = unitOfWork.FindDatabase(targetDbContextType);
            if (database == null)
            {
                database = new MongoDbDatabase(
                    await CreateDbContextAsync<TDbContext>(
                        unitOfWork,
                        dbConfig,
                        databaseName,
                        cancellationToken
                    )
                );

                unitOfWork.AddDatabase(targetDbContextType, database);
            }

            return (TDbContext)((MongoDbDatabase) database).DbContext;
        }
        
        protected TDbContext CreateDbContext<TDbContext>(
            IUnitOfWork unitOfWork,
            MongoDbContextConfig dbConfig,
            string databaseName) where TDbContext: IMongoDbContext
        {
            var client = new MongoClient(dbConfig.Settings);
            var database = client.GetDatabase(databaseName);

            if (unitOfWork.IsTransactional)
            {
                return CreateDbContextWithTransaction<TDbContext>(
                    unitOfWork,
                    dbConfig,
                    client,
                    database
                );
            }

            var dbContext = Activator.CreateInstance<TDbContext>();
            dbContext.InitializeDatabase(database, client, null);

            return dbContext;
        }
        
        protected async Task<TDbContext> CreateDbContextAsync<TDbContext>(
            IUnitOfWork unitOfWork,
            MongoDbContextConfig dbConfig,
            string databaseName,
            CancellationToken cancellationToken = default) where TDbContext: IMongoDbContext
        {
            var client = new MongoClient(dbConfig.Settings);
            var database = client.GetDatabase(databaseName);

            if (unitOfWork.IsTransactional)
            {
                return await CreateDbContextWithTransactionAsync<TDbContext>(
                    unitOfWork,
                    dbConfig,
                    client,
                    database,
                    cancellationToken
                );
            }

            var dbContext = Activator.CreateInstance<TDbContext>();
            dbContext.InitializeDatabase(database, client, null);

            return dbContext;
        }
        
        protected TDbContext CreateDbContextWithTransaction<TDbContext>(
            IUnitOfWork unitOfWork,
            MongoDbContextConfig dbConfig,
            MongoClient client,
            IMongoDatabase database,
            CancellationToken cancellationToken = default) where TDbContext: IMongoDbContext
        {
            var session = client.StartSession();

            if (dbConfig.Timeout.HasValue)
            {
                session.AdvanceOperationTime(new BsonTimestamp(dbConfig.Timeout.Value));
            }

            session.StartTransaction();
            
            var dbContext = Activator.CreateInstance<TDbContext>();

            dbContext.InitializeDatabase(database, client, session);

            return dbContext;
        }
        
        protected async Task<TDbContext> CreateDbContextWithTransactionAsync<TDbContext>(
            IUnitOfWork unitOfWork,
            MongoDbContextConfig dbConfig,
            MongoClient client,
            IMongoDatabase database,
            CancellationToken cancellationToken = default) where TDbContext: IMongoDbContext
        {
            var session = await client.StartSessionAsync(cancellationToken: cancellationToken);

            if (dbConfig.Timeout.HasValue)
            {
                session.AdvanceOperationTime(new BsonTimestamp(dbConfig.Timeout.Value));
            }

            session.StartTransaction();
            
            var dbContext = Activator.CreateInstance<TDbContext>();

            dbContext.InitializeDatabase(database, client, session);

            return dbContext;
        }
        
        protected MongoDbContextConfig ResolveDbConfig(Type dbContextType)
        {
            var config = _mongoDbContextOptions.DbContextConfigs.GetOrDefault(dbContextType);
            if (config == null)
            {
                throw new MongoDbException(
                    $"{dbContextType.Name} is not set");
            }

            return config;
        }
    }
}