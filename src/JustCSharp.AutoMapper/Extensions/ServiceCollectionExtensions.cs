using JustCSharp.AutoMapper.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace JustCSharp.AutoMapper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJustCSharpMappers(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAutoMapper(typeof(CoreAutoMapperProfile));
            return serviceCollection;
        }
    }
}