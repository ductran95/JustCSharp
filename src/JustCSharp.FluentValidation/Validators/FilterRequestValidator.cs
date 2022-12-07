using FluentValidation;
using JustCSharp.Data.Requests;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.FluentValidation.Validators
{
    public class FilterRequestValidator: AbstractValidator<FilterRequest>
    {
        public FilterRequestValidator()
        {
            RuleFor(x => x.Field).NotEmpty();
        }
    }
}