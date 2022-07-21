using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JustCSharp.AspNetCore.Module.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region IApplicationBuilder

        public static void RegisterModulesFromExecutingAssembly(this IApplicationBuilder applicationBuilder, IHostEnvironment environment, IConfiguration configuration)
        {
            RegisterModulesFromAssemblies(applicationBuilder, configuration, environment, Assembly.GetExecutingAssembly());
        }
        
        public static void RegisterModulesFromAssemblyContaining<TType>(this IApplicationBuilder applicationBuilder, IHostEnvironment environment, IConfiguration configuration)
        {
            RegisterModulesFromAssembliesContaining(applicationBuilder, configuration, environment, typeof(TType));
        }
        
        public static void RegisterModulesFromAssembliesContaining(this IApplicationBuilder applicationBuilder, IConfiguration configuration, IHostEnvironment environment, params Type[] types)
        {
            var assemblies = types.Select(x => x.Assembly).ToArray();
            RegisterModulesFromAssemblies(applicationBuilder, configuration, environment, assemblies);
        }
        
        public static void RegisterModulesFromAssemblies(this IApplicationBuilder serviceCollection, IConfiguration applicationBuilder, IHostEnvironment environment, params Assembly[] assemblies)
        {
            var moduleTypes = assemblies.SelectMany(a => a.DefinedTypes
                .Where(x => typeof(IApplicationBuilderModule).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract));
            
            var moduleInstances = moduleTypes.Select(Activator.CreateInstance).Cast<IApplicationBuilderModule>()
                .OrderBy(x => x.Order).ThenBy(x=>x.GetType().Name);
            
            foreach (var moduleInstance in moduleInstances)
            {
                moduleInstance.Register(serviceCollection, applicationBuilder, environment);
            }
        }

        #endregion

#if NET6_0_OR_GREATER
        #region WebApplication

        public static void RegisterModulesFromExecutingAssembly(this WebApplication app)
        {
            RegisterModulesFromAssemblies(app, Assembly.GetExecutingAssembly());
        }
        
        public static void RegisterModulesFromAssemblyContaining<TType>(this WebApplication app)
        {
            RegisterModulesFromAssembliesContaining(app, typeof(TType));
        }
        
        public static void RegisterModulesFromAssembliesContaining(this WebApplication app, params Type[] types)
        {
            var assemblies = types.Select(x => x.Assembly).ToArray();
            RegisterModulesFromAssemblies(app, assemblies);
        }
        
        public static void RegisterModulesFromAssemblies(this WebApplication app, params Assembly[] assemblies)
        {
            var moduleTypes = assemblies.SelectMany(a => a.DefinedTypes
                .Where(x => typeof(IWebApplicationModule).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract));
            
            var moduleInstances = moduleTypes.Select(Activator.CreateInstance).Cast<IWebApplicationModule>()
                .OrderBy(x => x.Order).ThenBy(x=>x.GetType().Name);
            
            foreach (var moduleInstance in moduleInstances)
            {
                moduleInstance.Register(app);
            }
        }

        #endregion
#endif
    }
}