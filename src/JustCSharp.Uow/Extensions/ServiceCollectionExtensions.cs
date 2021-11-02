using JustCSharp.Core.Extensions;
using JustCSharp.Uow.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Uow.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUow<TUnitOfWork, TUnitOfWorkProvider>(this IServiceCollection serviceCollection)
            where TUnitOfWork: class, IUnitOfWork where TUnitOfWorkProvider: class, IUnitOfWorkProviderOfT<TUnitOfWork>
        {
            AddUow<TUnitOfWorkProvider>(serviceCollection);
            
            serviceCollection.TryAddScoped<IUnitOfWorkProviderOfT<TUnitOfWork>, TUnitOfWorkProvider>();
            serviceCollection.TryAddScoped<TUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkProviderOfT<TUnitOfWork>>().UnitOfWorkOfT);
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddUow<TUnitOfWorkProvider>(this IServiceCollection serviceCollection)
            where TUnitOfWorkProvider: class, IUnitOfWorkProvider
        {
            AddUowCore(serviceCollection);
            
            serviceCollection.TryAddScoped<IUnitOfWorkProvider, TUnitOfWorkProvider>();
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddUow(this IServiceCollection serviceCollection)
        {
            AddUowCore(serviceCollection);
            
            serviceCollection.TryAddScoped<IUnitOfWorkProvider, UnitOfWorkProvider>();
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddUowCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddJustCSharpCore();
            
            serviceCollection.TryAddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkProvider>().UnitOfWork);
            
            return serviceCollection;
        }
    }
}