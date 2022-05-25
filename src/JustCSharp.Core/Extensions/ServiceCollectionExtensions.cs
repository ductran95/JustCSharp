using JustCSharp.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJustCSharpCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<ILazyServiceProvider, LazyServiceProvider>();
            return serviceCollection;
        }
    }
}