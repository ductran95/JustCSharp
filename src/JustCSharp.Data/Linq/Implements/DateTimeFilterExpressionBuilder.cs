using System;
using System.Linq.Expressions;
using JustCSharp.Data.Enums;
using JustCSharp.Data.Requests;

namespace JustCSharp.Data.Linq.Implements;

internal class DateTimeEqualFilterExpressionBuilder: FilterExpressionBuilder<FilterRequest<DateTime>>
{
    protected override bool IsSatisfied(FilterRequest<DateTime> filterRequest)
    {
        return filterRequest.Operator == FilterOperator.Equal;
    }

    protected override Expression<Func<TEntity, bool>> BuildExpression<TEntity>(FilterRequest<DateTime> filterRequest)
    {
        throw new NotImplementedException();
    }
}