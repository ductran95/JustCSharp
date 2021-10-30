using System;
using System.Diagnostics.CodeAnalysis;
using JustCSharp.MongoDB.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using JustCSharp.Utility.Extensions;

namespace JustCSharp.MongoDB.Model
{
    public class MongoDbContextOptionsBuilder<TDbContext> where TDbContext: IMongoDbContext
    {
        private IServiceProvider _applicationServiceProvider;
        private MongoDbContextOptions _options;

        public MongoDbContextOptions Options => _options;
        public Type DbContextType => typeof(TDbContext);

        public MongoDbContextOptionsBuilder()
            : this(new MongoDbContextOptions())
        {
        }

        public MongoDbContextOptionsBuilder([NotNull] MongoDbContextOptions options)
        {
            _options = options;
        }

        public void UseApplicationServiceProvider(IServiceProvider serviceProvider)
        {
            this._applicationServiceProvider = serviceProvider;
        }
        
        public MongoDbContextOptionsBuilder<TDbContext> UseConnectionStringName([NotNull] string connectionStringName)
        {
            var connection = _applicationServiceProvider.GetRequiredService<IConfiguration>();
            var connectionString = connection.GetConnectionString(connectionStringName);
            var config = _options.DbContextConfigs.GetOrAdd(typeof(TDbContext), () => new MongoDbContextConfig
            {
                DbContextType = DbContextType,
            });
            config.ConnectionStringName = connectionStringName;
            config.ConnectionString = connectionString;

            config.Settings = MongoClientSettings.FromConnectionString(config.ConnectionString);

            return this;
        }

        public MongoDbContextOptionsBuilder<TDbContext> UseConnectionString([NotNull] string connectionString)
        {
            var config = _options.DbContextConfigs.GetOrAdd(typeof(TDbContext), () => new MongoDbContextConfig
            {
                DbContextType = DbContextType,
            });
            config.ConnectionString = connectionString;
            
            config.Settings = MongoClientSettings.FromConnectionString(config.ConnectionString);

            return this;
        }
        
        public MongoDbContextOptionsBuilder<TDbContext> UseTimeout(int timeout)
        {
            var config = _options.DbContextConfigs.GetOrAdd(typeof(TDbContext), () => new MongoDbContextConfig
            {
                DbContextType = DbContextType,
            });
            config.Timeout = timeout;

            return this;
        }
        
        public MongoDbContextOptionsBuilder<TDbContext> UseSettings(Action<MongoClientSettings> settingAction)
        {
            var config = _options.DbContextConfigs.GetOrAdd(typeof(TDbContext), () => new MongoDbContextConfig
            {
                DbContextType = DbContextType,
            });
            
            settingAction?.Invoke(config.Settings);

            return this;
        }
    }
}