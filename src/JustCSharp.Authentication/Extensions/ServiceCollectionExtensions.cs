using JustCSharp.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Authentication.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJustCSharpAuthContext<TAuthContext, TAuthContextProvider>(this IServiceCollection serviceCollection) 
            where TAuthContext: class, IAuthContext where TAuthContextProvider: class, IAuthContextProviderOfT<TAuthContext>
        {
            AddJustCSharpAuthContextCore(serviceCollection);
            
            serviceCollection.TryAddScoped<IAuthContextProviderOfT<TAuthContext>, TAuthContextProvider>();
            serviceCollection.TryAddScoped<IAuthContextProvider>(sp => sp.GetRequiredService<IAuthContextProviderOfT<TAuthContext>>());
            serviceCollection.TryAddScoped<TAuthContext>(sp => sp.GetRequiredService<IAuthContextProviderOfT<TAuthContext>>().AuthContextOfT);
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddJustCSharpAuthContext<TAuthContextProvider>(this IServiceCollection serviceCollection) 
            where TAuthContextProvider: class, IAuthContextProvider
        {
            AddJustCSharpAuthContextCore(serviceCollection);
            
            serviceCollection.TryAddScoped<IAuthContextProvider, TAuthContextProvider>();
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddJustCSharpAuthContext(this IServiceCollection serviceCollection)
        {
            AddJustCSharpAuthContextCore(serviceCollection);
            
            serviceCollection.TryAddScoped<IAuthContextProvider, AuthContextProvider>();
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddJustCSharpAuthContextCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddJustCSharpCore();
            
            serviceCollection.TryAddScoped<IAuthContext>(sp => sp.GetRequiredService<IAuthContextProvider>().AuthContext);
            
            return serviceCollection;
        }
    }
}