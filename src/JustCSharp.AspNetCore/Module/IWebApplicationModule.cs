using Microsoft.AspNetCore.Builder;

#if NET6_0_OR_GREATER
namespace JustCSharp.AspNetCore.Module
{
    public interface IWebApplicationModule
    {
        /// <summary>
        /// Order of registering
        /// </summary>
        int Order => -1;
        
        /// <summary>
        /// Register services
        /// </summary>
        /// <param name="app"></param>
        void Register(WebApplication app);
    }
}
#endif
