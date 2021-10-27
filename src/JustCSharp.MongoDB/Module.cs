using JustCSharp.Core.Module;
using JustCSharp.MongoDB.Extensions;
using JustCSharp.Uow.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace JustCSharp.Uow
{
    public class Module: IModule
    {
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration, IHostEnvironment environment)
        {
            serviceCollection.AddMongoDbCore();
        }
    }
}