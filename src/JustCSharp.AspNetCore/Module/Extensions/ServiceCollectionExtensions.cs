using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace JustCSharp.AspNetCore.Module.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterModule<TModule>(this WebApplication app) where TModule : IAspNetCoreModule
        {
            var module = Activator.CreateInstance<TModule>();
            module.Register(app);
        }

        public static void RegisterModulesFromExecutingAssembly(this WebApplication app)
        {
            RegisterModulesFromAssemblies(app, Assembly.GetExecutingAssembly());
        }

        public static void RegisterModulesFromAssemblyContaining<TType>(this WebApplication app)
        {
            RegisterModulesFromAssembliesContaining(app, typeof(TType));
        }

        private static void RegisterModulesFromAssembliesContaining(this WebApplication app, params Type[] types)
        {
            var assemblies = types.Select(x => x.Assembly).ToArray();
            RegisterModulesFromAssemblies(app, assemblies);
        }

        private static void RegisterModulesFromAssemblies(this WebApplication app, params Assembly[] assemblies)
        {
            var moduleTypes = assemblies.SelectMany(a => a.DefinedTypes
                .Where(x => typeof(IAspNetCoreModule).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract));

            var moduleInstances = moduleTypes.Select(Activator.CreateInstance).Cast<IAspNetCoreModule>()
                .OrderBy(x => x.Order).ThenBy(x => x.GetType().Name);

            foreach (var moduleInstance in moduleInstances)
            {
                moduleInstance.Register(app);
            }
        }
    }
}