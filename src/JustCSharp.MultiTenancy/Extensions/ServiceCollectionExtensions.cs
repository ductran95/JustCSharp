using JustCSharp.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.MultiTenancy.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJustCSharpTenantContext<TTenantContext, TTenantContextProvider>(this IServiceCollection serviceCollection) 
            where TTenantContext: class, ITenantContext where TTenantContextProvider: class, ITenantContextProviderOfT<TTenantContext>
        {
            AddJustCSharpTenantContext<TTenantContextProvider>(serviceCollection);
            
            serviceCollection.TryAddScoped<ITenantContextProviderOfT<TTenantContext>, TTenantContextProvider>();
            serviceCollection.TryAddScoped<TTenantContext>(sp => sp.GetRequiredService<ITenantContextProviderOfT<TTenantContext>>().TenantContextOfT);
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddJustCSharpTenantContext<TTenantContextProvider>(this IServiceCollection serviceCollection) 
            where TTenantContextProvider: class, ITenantContextProvider
        {
            AddJustCSharpTenantContextCore(serviceCollection);
            
            serviceCollection.TryAddScoped<ITenantContextProvider, TTenantContextProvider>();
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddJustCSharpTenantContext(this IServiceCollection serviceCollection)
        {
            AddJustCSharpTenantContextCore(serviceCollection);
            
            serviceCollection.TryAddScoped<ITenantContextProvider, TenantContextProvider>();
            
            return serviceCollection;
        }
        
        public static IServiceCollection AddJustCSharpTenantContextCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddJustCSharpCore();
            
            serviceCollection.TryAddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContextProvider>().TenantContext);
            
            return serviceCollection;
        }
    }
}