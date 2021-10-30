using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JustCSharp.Module
{
    public interface IModule
    {
        /// <summary>
        /// Order of registering
        /// </summary>
        int Order => -1;
        
        /// <summary>
        /// Register services
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="environment">Hosting environment, nullable</param>
        void Register(IServiceCollection serviceCollection, IConfiguration configuration, IHostEnvironment environment);
    }
}