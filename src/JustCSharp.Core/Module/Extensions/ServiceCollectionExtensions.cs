using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JustCSharp.Core.Module.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Hosting

        public static void RegisterModulesFromExecutingAssembly(this IServiceCollection serviceCollection, IHostEnvironment environment, IConfiguration configuration)
        {
            RegisterModulesFromAssemblies(serviceCollection, configuration, environment, Assembly.GetExecutingAssembly());
        }
        
        public static void RegisterModulesFromAssemblyContaining<TType>(this IServiceCollection serviceCollection, IHostEnvironment environment, IConfiguration configuration)
        {
            RegisterModulesFromAssembliesContaining(serviceCollection, configuration, environment, typeof(TType));
        }
        
        public static void RegisterModulesFromAssembliesContaining(this IServiceCollection serviceCollection, IConfiguration configuration, IHostEnvironment environment, params Type[] types)
        {
            var assemblies = types.Select(x => x.Assembly).ToArray();
            RegisterModulesFromAssemblies(serviceCollection, configuration, environment, assemblies);
        }
        
        public static void RegisterModulesFromAssemblies(this IServiceCollection serviceCollection, IConfiguration configuration, IHostEnvironment environment, params Assembly[] assemblies)
        {
            var moduleTypes = assemblies.SelectMany(a => a.DefinedTypes
                .Where(x => typeof(IModule).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract));
            
            var moduleInstances = moduleTypes.Select(Activator.CreateInstance).Cast<IModule>()
                .OrderBy(x => x.Order).ThenBy(x=>x.GetType().Name);
            
            foreach (var moduleInstance in moduleInstances)
            {
                moduleInstance.Register(serviceCollection, configuration, environment);
            }
        }

        #endregion

        #region No Hosting

        public static void RegisterModulesFromExecutingAssembly(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            RegisterModulesFromAssemblies(serviceCollection, configuration, Assembly.GetExecutingAssembly());
        }
        
        public static void RegisterModulesFromAssemblyContaining<TType>(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            RegisterModulesFromAssembliesContaining(serviceCollection, configuration, typeof(TType));
        }
        
        public static void RegisterModulesFromAssembliesContaining(this IServiceCollection serviceCollection, IConfiguration configuration, params Type[] types)
        {
            var assemblies = types.Select(x => x.Assembly).ToArray();
            RegisterModulesFromAssemblies(serviceCollection, configuration, assemblies);
        }
        
        public static void RegisterModulesFromAssemblies(this IServiceCollection serviceCollection, IConfiguration configuration, params Assembly[] assemblies)
        {
            RegisterModulesFromAssemblies(serviceCollection, configuration, null, assemblies);
        }

        #endregion
        
    }
}