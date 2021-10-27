using JustCSharp.Uow.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Uow.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUowCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IUnitOfWorkProvider, UnitOfWorkProvider>();
            serviceCollection.TryAddScoped<IUnitOfWorkAccessor, UnitOfWorkAccessor>();
            serviceCollection.TryAddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkAccessor>().UnitOfWork);
            
            return serviceCollection;
        }
    }
}