using System;
using System.Linq.Expressions;
using JustCSharp.Data.Requests;

namespace JustCSharp.Data.Linq;

public interface IFilterExpressionBuilder
{
    bool IsSatisfied(IFilterRequest filterRequest);
    Expression<Func<TEntity, bool>> BuildExpression<TEntity>(IFilterRequest filterRequest);
}