using FluentValidation;
using JustCSharp.Data.Requests;

namespace JustCSharp.FluentValidation.Validators
{
    public class SortRequestValidator: AbstractValidator<SortRequest>
    {
        public SortRequestValidator()
        {
            RuleFor(x => x.Field).NotEmpty();
        }
    }
}