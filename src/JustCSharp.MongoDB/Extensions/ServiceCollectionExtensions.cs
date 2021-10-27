using System;
using JetBrains.Annotations;
using JustCSharp.MongoDB.Context;
using JustCSharp.MongoDB.Model;
using JustCSharp.Uow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.MongoDB.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbContext<TDbContext>(
            this IServiceCollection serviceCollection, 
            [CanBeNull] Action<MongoDbContextOptionsBuilder<TDbContext>> optionsAction) where TDbContext: MongoDbContext
        {
            serviceCollection.AddMongoDbCore();
            serviceCollection.TryAddScoped<TDbContext>(sp =>
                sp.GetRequiredService<IMongoDbContextProvider>().GetDbContext<TDbContext>());
            
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(MongoDbContextOptions),
                    sp => CreateDbContextOptions(sp, optionsAction),
                    ServiceLifetime.Singleton)
            );
            
            return serviceCollection;
        }

        public static IServiceCollection AddMongoDbCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IMongoDbContextProvider, UowMongoDbContextProvider>();
            serviceCollection.TryAddSingleton(typeof(IRepository<>), sp => sp.GetRequiredService(typeof(IRepository<>)));
            
            return serviceCollection;
        }
        
        private static MongoDbContextOptions CreateDbContextOptions<TDbContext>(
            [NotNull] IServiceProvider applicationServiceProvider,
            [CanBeNull] Action<MongoDbContextOptionsBuilder<TDbContext>> optionsAction) where TDbContext: MongoDbContext
        {
            var options = applicationServiceProvider.GetService<MongoDbContextOptions>();
            MongoDbContextOptionsBuilder<TDbContext> builder;
            
            if (options == null)
            {
                builder = new MongoDbContextOptionsBuilder<TDbContext>();
            }
            else
            {
                builder = new MongoDbContextOptionsBuilder<TDbContext>(options);
            }

            builder.UseApplicationServiceProvider(applicationServiceProvider);

            optionsAction?.Invoke(builder);

            return builder.Options;
        }
    }
}