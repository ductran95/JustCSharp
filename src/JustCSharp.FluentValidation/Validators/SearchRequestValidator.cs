using FluentValidation;
using JustCSharp.Data.Requests;

namespace JustCSharp.FluentValidation.Validators
{
    public class SearchRequestValidator: AbstractValidator<SearchRequest>
    {
        public SearchRequestValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0);
            RuleForEach(x => x.Filters).SetValidator(new FilterRequestValidator());
            RuleForEach(x => x.Sorts).SetValidator(new SortRequestValidator());
        }
    }
}