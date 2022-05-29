using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using JustCSharp.Core.Exceptions;
using JustCSharp.FluentValidation.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JustCSharp.Api.FluentValidation.Attributes;

public class RequestValidationAttribute : ActionFilterAttribute
{
    public RequestValidationAttribute()
    {
        Order = 2;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Result == null && !context.ModelState.IsValid)
        {
            var errors = new List<ValidationFailure>();

            foreach (var prop in context.ModelState.ToDictionary(x => x.Key, x => x.Value))
                errors.AddRange(prop.Value.Errors.Select(x => new ValidationFailure(prop.Key, x.ErrorMessage)));

            if (errors.Any()) throw new BadRequestException(errors.Select(x => x.ToError()));
        }
    }
}