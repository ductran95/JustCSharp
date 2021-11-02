using System;
using JustCSharp.Core.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JustCSharp.Core.Logging.Extensions
{
    public static class LazyServiceProviderExtensions
    {
        public static ILogger GetLogger(this ILazyServiceProvider lazyServiceProvider, Type serviceType)
        {
            var loggerFactory = lazyServiceProvider.LazyGetService<ILoggerFactory>();
            return lazyServiceProvider.LazyGetService<ILogger>(provider => loggerFactory?.CreateLogger(serviceType.FullName) ?? NullLogger.Instance);
        }
    }
}