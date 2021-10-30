using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Authentication.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthContext<TAuthContext, TAuthContextProvider>(this IServiceCollection serviceCollection) 
            where TAuthContext: AuthContextBase where TAuthContextProvider: class, IAuthContextProvider<TAuthContext>
        {
            AddAuthContextCore(serviceCollection);
            
            serviceCollection.TryAddSingleton<IAuthContextProvider, TAuthContextProvider>();
            serviceCollection.TryAddSingleton<IAuthContextProvider<TAuthContext>, TAuthContextProvider>();
            serviceCollection.TryAddScoped<TAuthContext>(sp => sp.GetRequiredService<AuthContextBase>() as TAuthContext);

            
            return serviceCollection;
        }
        
        public static IServiceCollection AddAuthContextCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<IAuthContextAccessor, AuthContextAccessor>();
            serviceCollection.TryAddScoped<AuthContextBase>(sp => sp.GetRequiredService<IAuthContextAccessor>().AuthContext);
            
            return serviceCollection;
        }
    }
}