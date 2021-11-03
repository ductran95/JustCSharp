using FluentValidation;
using JustCSharp.FluentValidation.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace JustCSharp.FluentValidation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJustCSharpValidators(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddValidatorsFromAssemblyContaining(typeof(SearchRequestValidator));
            return serviceCollection;
        }
    }
}