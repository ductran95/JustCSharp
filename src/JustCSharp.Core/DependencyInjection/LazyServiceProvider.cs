using System;
using System.Collections.Generic;
using JustCSharp.Utility.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace JustCSharp.Core.DependencyInjection
{
    public class LazyServiceProvider: ILazyServiceProvider
    {
        protected readonly IServiceProvider _serviceProvider;
        
        protected Dictionary<Type, object> CachedServices { get; set; }

        public LazyServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            CachedServices = new Dictionary<Type, object>();
        }

        public virtual T LazyGetRequiredService<T>()
        {
            return (T) LazyGetRequiredService(typeof(T));
        }

        public virtual object LazyGetRequiredService(Type serviceType)
        {
            return CachedServices.GetOrAdd(serviceType, () => _serviceProvider.GetRequiredService(serviceType));
        }

        public virtual T LazyGetService<T>()
        {
            return (T) LazyGetService(typeof(T));
        }

        public virtual object LazyGetService(Type serviceType)
        {
            return CachedServices.GetOrAdd(serviceType, () => _serviceProvider.GetService(serviceType));
        }

        public virtual T LazyGetService<T>(T defaultValue)
        {
            return (T) LazyGetService(typeof(T), defaultValue);
        }

        public virtual object LazyGetService(Type serviceType, object defaultValue)
        {
            return LazyGetService(serviceType) ?? defaultValue;
        }

        public virtual T LazyGetService<T>(Func<IServiceProvider, object> factory)
        {
            return (T) LazyGetService(typeof(T), factory);
        }

        public virtual object LazyGetService(Type serviceType, Func<IServiceProvider, object> factory)
        {
            return CachedServices.GetOrAdd(serviceType, () => factory(_serviceProvider));
        }
    }
}