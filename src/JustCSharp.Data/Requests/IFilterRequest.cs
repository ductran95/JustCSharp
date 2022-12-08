using System;
using System.Linq.Expressions;

namespace JustCSharp.Data.Requests;

public interface IFilterRequest
{
    Expression<Func<TEntity, bool>> ToExpression<TEntity>();
}