using System;
using System.Linq.Expressions;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.Data.Requests
{
    public class SortRequest: IRequest
    {
        public string Field { get; set; } = default!;
        public bool Asc { get; set; }
        
        public Expression<Func<TEntity, object>>? ToExpression<TEntity>()
        {
            var pi = typeof(TEntity).GetProperty(Field);

            if (pi == null)
            {
                return null;
            }
            var paramExp = Expression.Parameter(typeof(TEntity), "p");
            var propertyExp = Expression.Property(paramExp, pi);
            var convertedExp = Expression.Convert(propertyExp, typeof(object));

            return Expression.Lambda<Func<TEntity, object>>(convertedExp, paramExp);
        }
        
        public Expression<Func<TEntity, TData>>? ToExpression<TEntity, TData>()
        {
            var pi = typeof(TEntity).GetProperty(Field);

            if (pi == null)
            {
                return null;
            }
            var paramExp = Expression.Parameter(typeof(TEntity), "p");
            var propertyExp = Expression.Property(paramExp, pi);

            return Expression.Lambda<Func<TEntity, TData>>(propertyExp, paramExp);
        }
    }
}