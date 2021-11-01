using System;
using JetBrains.Annotations;
using JustCSharp.Data.Entities;
using JustCSharp.Database.MongoDB.Context;
using JustCSharp.Database.MongoDB.Model;
using JustCSharp.Database.MongoDB.Repositories;
using JustCSharp.Uow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Database.MongoDB.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbContext<TDbContext>(
            this IServiceCollection serviceCollection, 
            [CanBeNull] Action<MongoDbContextOptionsBuilder<TDbContext>> optionsAction) where TDbContext: MongoDbContext
        {
            serviceCollection.AddMongoDbCore();
            serviceCollection.TryAddScoped<TDbContext>();
            
            serviceCollection.AddSingleton<MongoDbContextOptions<TDbContext>>(sp => CreateDbContextOptions(sp, optionsAction));
            
            return serviceCollection;
        }

        public static IServiceCollection AddMongoDbRepository<TRepository, TEntity>(this IServiceCollection serviceCollection)
            where TRepository : class, IMongoDbRepository<TEntity> where TEntity: IEntity
        {
            serviceCollection.TryAddScoped<IMongoDbRepository<TEntity>, TRepository>();
            serviceCollection.TryAddScoped<IRepository<TEntity>>(sp => sp.GetRequiredService<IMongoDbRepository<TEntity>>());
            return serviceCollection;
        }

        public static IServiceCollection AddMongoDbCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IMongoEntityModelCache, MongoEntityModelCache>();
            
            return serviceCollection;
        }
        
        private static MongoDbContextOptions<TDbContext> CreateDbContextOptions<TDbContext>(
            [NotNull] IServiceProvider applicationServiceProvider,
            [CanBeNull] Action<MongoDbContextOptionsBuilder<TDbContext>> optionsAction) where TDbContext: MongoDbContext
        {
            MongoDbContextOptionsBuilder<TDbContext> builder = new MongoDbContextOptionsBuilder<TDbContext>();
            
            builder.UseApplicationServiceProvider(applicationServiceProvider);

            optionsAction?.Invoke(builder);

            return builder.Options;
        }
    }
}