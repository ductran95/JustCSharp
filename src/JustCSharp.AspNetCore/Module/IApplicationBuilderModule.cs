using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JustCSharp.AspNetCore.Module
{
    public interface IApplicationBuilderModule
    {
        /// <summary>
        /// Order of registering
        /// </summary>
        int Order => -1;
        
        /// <summary>
        /// Register services
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <param name="configuration"></param>
        /// <param name="environment">Hosting environment, nullable</param>
        void Register(IApplicationBuilder applicationBuilder, IConfiguration configuration, IHostEnvironment environment);
    }
}