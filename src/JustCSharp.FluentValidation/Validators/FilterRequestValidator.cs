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
            RuleFor(x => x).Must(x => OperatorHelper.OnlyOneTrue(
                !string.IsNullOrEmpty(x.ValueString),
                x.ValueGuid != null,
                x.ValueDateTimeFrom != null || x.ValueDateTimeTo != null,
                x.ValueNumberFrom != null || x.ValueNumberTo != null,
                x.ValueBool != null,
                x.ValueList != null
            ));
        }
    }
}