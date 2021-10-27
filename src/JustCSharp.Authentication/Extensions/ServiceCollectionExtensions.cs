using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Authentication.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticationCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<IAuthContextAccessor, AuthContextAccessor>();
            serviceCollection.TryAddScoped<AuthContextBase>(sp => sp.GetRequiredService<IAuthContextAccessor>().AuthContext);
            
            return serviceCollection;
        }
    }
}