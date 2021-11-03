using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using JustCSharp.Core.Exceptions;
using JustCSharp.Data;
using JustCSharp.Data.Constants;
using JustCSharp.Data.Requests;
using JustCSharp.Utility.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace JustCSharp.FluentValidation.Extensions
{
    public static class ValidatorExtensions
    {
        public static IEnumerable<ValidationFailure> Validate<TData>(this SearchRequest searchRequest, bool throwException = true)
        {
            var requestName = searchRequest.GetGenericTypeName();
            
            var properties = typeof(TData).GetProperties(ReflectionConstants.SearchPropertyFlags);
            var errors = new List<ValidationFailure>();

            if (searchRequest.Filters != null && searchRequest.Filters.Any())
            {
                for (int i = 0; i < searchRequest.Filters.Count; i++)
                {
                    var filterRequest = searchRequest.Filters[i];
                    var property = properties.FirstOrDefault(x =>
                        x.Name.Equals(filterRequest.Field, StringComparison.InvariantCultureIgnoreCase));

                    if (property == null)
                    {
                        errors.Add(new ValidationFailure($"Filters.[{i}]", "Field not found"));
                        continue;
                    }

                    filterRequest.Field = property.Name;

                    // TODO; validate property type
                }
            }
            
            if (searchRequest.Sorts != null && searchRequest.Sorts.Any())
            {
                for (int i = 0; i < searchRequest.Sorts.Count; i++)
                {
                    var sortRequest = searchRequest.Sorts[i];
                    var property = properties.FirstOrDefault(x =>
                        x.Name.Equals(sortRequest.Field, StringComparison.InvariantCultureIgnoreCase));

                    if (property == null)
                    {
                        errors.Add(new ValidationFailure($"Sorts.[{i}]", "Field not found"));
                        continue;
                    }
                    
                    sortRequest.Field = property.Name;
                    
                    // TODO; validate property type
                }
            }
            
            if (throwException && errors.Any())
            {
                throw new BadRequestException(errors.Select(x=>x.ToError()), $"Validation Errors for {requestName}");
            }

            return errors;
        }
        
        public static IEnumerable<ValidationFailure> Validate<T>(this IServiceProvider serviceProvider, T data,
            bool throwException = true)
        {
            var validators = serviceProvider.GetServices<IValidator<T>>();
            return validators.Validate(data, throwException);
        }
        
        public static async Task<IEnumerable<ValidationFailure>> ValidateAsync<T>(this IServiceProvider serviceProvider, T data,
            bool throwException = true, CancellationToken cancellationToken = default)
        {
            var validators = serviceProvider.GetServices<IValidator<T>>();
            return await validators.ValidateAsync(data, throwException, cancellationToken);
        }

        public static IEnumerable<ValidationFailure> Validate<T>(this IEnumerable<IValidator<T>> validators,
            T data, bool throwException = true)
        {
            var requestName = data.GetGenericTypeName();
            
            if (validators.Any())
            {
                var errors = validators.Select(v => v.Validate(data)).SelectMany(x => x.Errors);

                if (throwException && errors.Any())
                {
                    throw new BadRequestException(errors.Select(x=>x.ToError()), $"Validation Errors for {requestName}");
                }

                return errors;
            }

            return new List<ValidationFailure>();
        }
        
        public static async Task<IEnumerable<ValidationFailure>> ValidateAsync<T>(this IEnumerable<IValidator<T>> validators,
            T data, bool throwException = true, CancellationToken cancellationToken = default)
        {
            var requestName = data.GetGenericTypeName();
            
            if (validators.Any())
            {
                var errors = await validators.Select(async v => await v.ValidateAsync(data, cancellationToken)).SelectManyAsync(x => x.Errors);

                if (throwException && errors.Any())
                {
                    throw new BadRequestException(errors.Select(x=>x.ToError()), $"Validation Errors for {requestName}");
                }

                return errors;
            }

            return new List<ValidationFailure>();
        }
        
        public static Error ToError(this ValidationFailure validationFailure)
        {
            return new Error(validationFailure.PropertyName, validationFailure.ErrorMessage);
        }
    }
}