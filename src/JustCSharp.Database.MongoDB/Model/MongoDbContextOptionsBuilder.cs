using System;
using System.Diagnostics.CodeAnalysis;
using JustCSharp.Database.MongoDB.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace JustCSharp.Database.MongoDB.Model
{
    public class MongoDbContextOptionsBuilder<TDbContext> where TDbContext: IMongoDbContext
    {
        private IServiceProvider _applicationServiceProvider;
        private MongoDbContextOptions<TDbContext> _options;

        public MongoDbContextOptions<TDbContext> Options => _options;
        public Type DbContextType => typeof(TDbContext);

        public void UseApplicationServiceProvider(IServiceProvider serviceProvider)
        {
            this._applicationServiceProvider = serviceProvider;
        }

        public MongoDbContextOptionsBuilder()
        {
            _options = new MongoDbContextOptions<TDbContext>();
        }
        
        public MongoDbContextOptionsBuilder<TDbContext> UseConnectionStringName([NotNull] string connectionStringName)
        {
            var connection = _applicationServiceProvider.GetRequiredService<IConfiguration>();
            var connectionString = connection.GetConnectionString(connectionStringName);
            
            _options.ConnectionStringName = connectionStringName;
            _options.ConnectionString = connectionString;

            _options.Settings = MongoClientSettings.FromConnectionString(_options.ConnectionString);

            return this;
        }

        public MongoDbContextOptionsBuilder<TDbContext> UseConnectionString([NotNull] string connectionString)
        {
            _options.ConnectionString = connectionString;
            
            _options.Settings = MongoClientSettings.FromConnectionString(_options.ConnectionString);

            return this;
        }
        
        public MongoDbContextOptionsBuilder<TDbContext> UseTimeout(int timeout)
        {
            _options.Timeout = timeout;

            return this;
        }
        
        public MongoDbContextOptionsBuilder<TDbContext> UseSettings(Action<MongoClientSettings> settingAction)
        {
            settingAction?.Invoke(_options.Settings);

            return this;
        }
    }
}