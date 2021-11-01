using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Authentication.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthContext<TAuthContext, TAuthContextProvider>(this IServiceCollection serviceCollection) 
            where TAuthContext: class, IAuthContext where TAuthContextProvider: class, IAuthContextProviderOfT<TAuthContext>
        {
            AddAuthContext<TAuthContextProvider>(serviceCollection);
            
            serviceCollection.TryAddScoped<IAuthContextProviderOfT<TAuthContext>, TAuthContextProvider>();
            serviceCollection.TryAddScoped<TAuthContext>(sp => sp.GetRequiredService<IAuthContextProviderOfT<TAuthContext>>().AuthContextOfT);
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddAuthContext<TAuthContextProvider>(this IServiceCollection serviceCollection) 
            where TAuthContextProvider: class, IAuthContextProvider
        {
            AddAuthContextCore(serviceCollection);
            
            serviceCollection.TryAddScoped<IAuthContextProvider, TAuthContextProvider>();
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddAuthContext(this IServiceCollection serviceCollection)
        {
            AddAuthContextCore(serviceCollection);
            
            serviceCollection.TryAddScoped<IAuthContextProvider, AuthContextProvider>();
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddAuthContextCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<IAuthContext>(sp => sp.GetRequiredService<IAuthContextProvider>().AuthContext);
            
            return serviceCollection;
        }
    }
}