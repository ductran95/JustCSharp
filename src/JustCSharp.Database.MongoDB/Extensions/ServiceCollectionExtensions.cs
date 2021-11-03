using System;
using JetBrains.Annotations;
using JustCSharp.Core.Extensions;
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
        public static IServiceCollection AddJustCSharpMongoDbContext<TDbContext>(
            this IServiceCollection serviceCollection, 
            [CanBeNull] Action<MongoDbContextOptionsBuilder<TDbContext>> optionsAction) where TDbContext: MongoDbContext
        {
            serviceCollection.AddJustCSharpMongoDbCore();
            serviceCollection.TryAddScoped<TDbContext>();
            
            serviceCollection.AddSingleton<MongoDbContextOptions<TDbContext>>(sp => CreateDbContextOptions(sp, optionsAction));
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddJustCSharpMongoDbRepository<TRepository, TEntity>(this IServiceCollection serviceCollection)
            where TRepository : class, IMongoDbRepository<TEntity> 
            where TEntity: class, IEntity
        {
            serviceCollection.TryAddScoped<IMongoDbRepository<TEntity>, TRepository>();
            serviceCollection.TryAddScoped<IRepository<TEntity>>(sp => sp.GetRequiredService<IMongoDbRepository<TEntity>>());
            return serviceCollection;
        }

        public static IServiceCollection AddJustCSharpMongoDbCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddJustCSharpCore();
            
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