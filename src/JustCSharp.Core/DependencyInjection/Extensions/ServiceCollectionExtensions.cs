using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.Core.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scan and try register all classes implementing interface which implement ISingleton as Singleton
        /// IScoped as Scoped and ITransient as Transient
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        /// <remarks>
        /// Use <see cref="TryAddInjectionFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> when registering a service
        /// implementation of a service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="AddInjectionFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddInjectionFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static IServiceCollection TryAddInjectionFromAssemblies(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies)
        {
            TryAddSingletonFromAssemblies(serviceCollection, assemblies);
            TryAddScopedFromAssemblies(serviceCollection, assemblies); 
            TryAddTransientFromAssemblies(serviceCollection, assemblies);

            return serviceCollection;
        }
        
        /// <summary>
        /// Scan and try register all classes implementing interface which implement ISingleton as Singleton
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        /// <remarks>
        /// Use <see cref="TryAddSingletonFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> when registering a service
        /// implementation of a service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="AddSingletonFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddSingletonFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static IServiceCollection TryAddSingletonFromAssemblies(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies)
        {
            return InvokeAddInjectionFromAssemblies<ISingleton>(serviceCollection, assemblies,
                ServiceLifetime.Singleton,
                ServiceCollectionDescriptorExtensions.TryAdd);
        }
        
        /// <summary>
        /// Scan and try register all classes implementing interface which implement IScoped as Scoped
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        /// <remarks>
        /// Use <see cref="TryAddScopedFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> when registering a service
        /// implementation of a service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="AddScopedFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddScopedFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static IServiceCollection TryAddScopedFromAssemblies(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies)
        {
            return InvokeAddInjectionFromAssemblies<IScoped>(serviceCollection, assemblies,
                ServiceLifetime.Scoped,
                ServiceCollectionDescriptorExtensions.TryAdd);;
        }
        
        /// <summary>
        /// Scan and try register all classes implementing interface which implement ITransient as Transient
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        /// <remarks>
        /// Use <see cref="TryAddTransientFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> when registering a service
        /// implementation of a service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="AddTransientFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddTransientFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static IServiceCollection TryAddTransientFromAssemblies(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies)
        {
            return InvokeAddInjectionFromAssemblies<ITransient>(serviceCollection, assemblies,
                ServiceLifetime.Transient,
                ServiceCollectionDescriptorExtensions.TryAdd);;
        }
        
        /// <summary>
        /// Scan and register all classes implementing interface which implement ISingleton as Singleton
        /// IScoped as Scoped and ITransient as Transient
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddInjectionFromAssemblies(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies)
        {
            AddSingletonFromAssemblies(serviceCollection, assemblies);
            AddScopedFromAssemblies(serviceCollection, assemblies); 
            AddTransientFromAssemblies(serviceCollection, assemblies);

            return serviceCollection;
        }
        
        /// <summary>
        /// Scan and register all classes implementing interface which implement ISingleton as Singleton
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddSingletonFromAssemblies(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies)
        {
            return InvokeAddInjectionFromAssemblies<ISingleton>(serviceCollection, assemblies,
                ServiceLifetime.Singleton,
                AddEnumerable);
        }
        
        /// <summary>
        /// Scan and register all classes implementing interface which implement IScoped as Scoped
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddScopedFromAssemblies(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies)
        {
            return InvokeAddInjectionFromAssemblies<IScoped>(serviceCollection, assemblies,
                ServiceLifetime.Scoped,
                AddEnumerable);;
        }
        
        /// <summary>
        /// Scan and register all classes implementing interface which implement ITransient as Transient
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddTransientFromAssemblies(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies)
        {
            return InvokeAddInjectionFromAssemblies<ITransient>(serviceCollection, assemblies,
                ServiceLifetime.Transient,
                AddEnumerable);;
        }

        /// <summary>
        /// Scan and try register all classes implementing T
        /// e.g. interface IOperation, class SumOperation, class DivideOperation
        /// then we have IEnumerable&lt;IOperation&gt; = List {SumOperation, DivideOperation} in DI
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
        /// <typeparam name="T">The interface to scan</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        /// <remarks>
        /// Use <see cref="TryAddEnumerableFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>, ServiceLifetime)"/> when registering a service
        /// implementation of a service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="AddEnumerableFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>, ServiceLifetime)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddEnumerableFromAssemblies(IServiceCollection, IEnumerable&lt;Assembly>, ServiceLifetime)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static IServiceCollection TryAddEnumerableFromAssemblies<T>(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies, ServiceLifetime lifetime)
        {
            return InvokeAddEnumerableFromAssemblies<T>(serviceCollection, assemblies, lifetime, 
                ServiceCollectionDescriptorExtensions.TryAddEnumerable);
        }

        /// <summary>
        /// Scan and register all classes implementing T
        /// e.g. interface IOperation, class SumOperation, class DivideOperation
        /// then we have IEnumerable&lt;IOperation&gt; = List {SumOperation, DivideOperation} in DI
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assemblies">The <see cref="Assembly"/>s.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
        /// <typeparam name="T">The interface to scan</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddEnumerableFromAssemblies<T>(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies, ServiceLifetime lifetime)
        {
            return InvokeAddEnumerableFromAssemblies<T>(serviceCollection, assemblies, lifetime, AddEnumerable);
        }

        private static IServiceCollection InvokeAddEnumerableFromAssemblies<T>(
            this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies, ServiceLifetime lifetime,
            Action<IServiceCollection, ServiceDescriptor> action)
        {
            var interfaceType = typeof(T);

            foreach (var assembly in assemblies)
            {
                var implementationTypes = assembly.GetTypes()
                    .Where(x => x.IsClass && !x.IsAbstract && interfaceType.IsAssignableFrom(x));

                foreach (var implementationType in implementationTypes)
                {
                    var serviceDescription = new ServiceDescriptor(interfaceType, implementationType, lifetime);
                    action.Invoke(serviceCollection, serviceDescription);
                }
            }

            return serviceCollection;
        }

        private static IServiceCollection InvokeAddInjectionFromAssemblies<T>(this IServiceCollection serviceCollection,
            [NotNull] IEnumerable<Assembly> assemblies, ServiceLifetime lifetime, Action<IServiceCollection, ServiceDescriptor> action)
        {
            var interfaceType = typeof(T);

            var serviceMappings = new Dictionary<Type, Type>();

            foreach (var assembly in assemblies)
            {
                var singletonInterfaceTypes =
                    assembly.GetTypes().Where(x => x.IsInterface && interfaceType.IsAssignableFrom(x));
                foreach (var singletonInterfaceType in singletonInterfaceTypes)
                {
                    serviceMappings.TryAdd(singletonInterfaceType, null);
                }
            }

            foreach (var assembly in assemblies)
            {
                foreach (var serviceMapping in serviceMappings.Where(x => x.Value == null))
                {
                    var implementationType = assembly.GetTypes()
                        .FirstOrDefault(x => x.IsClass && !x.IsAbstract && serviceMapping.Key.IsAssignableFrom(x));
                    
                    if (implementationType != null)
                    {
                        serviceMappings[serviceMapping.Key] = implementationType;
                        
                        var serviceDescription = new ServiceDescriptor(serviceMapping.Key, implementationType, lifetime);
                        action(serviceCollection, serviceDescription);
                    }
                }
            }

            return serviceCollection;
        }
        
        private static void AddEnumerable(this IServiceCollection serviceCollection, ServiceDescriptor descriptor)
        {
            serviceCollection.Add(descriptor);
        }
    }
}