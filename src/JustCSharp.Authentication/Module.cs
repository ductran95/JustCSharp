using JustCSharp.Authentication.Extensions;
using JustCSharp.Core.Module;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JustCSharp.Authentication
{
    public class Module: IModule
    {
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration, IHostEnvironment environment)
        {
            serviceCollection.AddAuthenticationCore();
        }
    }
}