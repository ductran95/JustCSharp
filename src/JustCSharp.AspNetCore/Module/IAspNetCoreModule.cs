using JustCSharp.Core.Module;
using Microsoft.AspNetCore.Builder;

namespace JustCSharp.AspNetCore.Module
{
    public interface IAspNetCoreModule: IModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        /// <param name="app"></param>
        void Register(WebApplication app);
    }
}
