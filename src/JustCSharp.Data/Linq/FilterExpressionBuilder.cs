using System;
using System.Linq.Expressions;
using JustCSharp.Data.Requests;

namespace JustCSharp.Data.Linq;

public abstract class FilterExpressionBuilder<TFilterRequest>: IFilterExpressionBuilder where TFilterRequest: IFilterRequest
{
    protected abstract bool IsSatisfied(TFilterRequest filterRequest);

    protected abstract Expression<Func<TEntity, bool>> BuildExpression<TEntity>(TFilterRequest filterRequest);
    
    public virtual bool IsSatisfied(IFilterRequest filterRequest)
    {
        if (filterRequest is TFilterRequest filterRequestOfType)
        {
            return IsSatisfied(filterRequestOfType);
        }

        return false;
    }

    public virtual Expression<Func<TEntity, bool>> BuildExpression<TEntity>(IFilterRequest filterRequest)
    {
        if (filterRequest is TFilterRequest filterRequestOfType)
        {
            return BuildExpression<TEntity>(filterRequestOfType);
        }

        throw new ArgumentException($"filterRequest must be of type {typeof(TFilterRequest)}");
    }
}