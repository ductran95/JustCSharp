using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JustCSharp.Data.Requests;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.Data.Linq
{
    public static class FilterRequestExtensions
    {
        public static Expression<Func<T, bool>>? ToExpression<T>(this IEnumerable<FilterRequest>? filters)
        {
            var filterRequests = filters as FilterRequest[] ?? filters?.ToArray();
            if (filterRequests == null || !filterRequests.Any())
            {
                return null;
            }

            var properties = typeof(T).GetProperties(ReflectionHelper.SearchPropertyFlags);
            var param = Expression.Parameter(typeof(T), "p");
            
            List<Expression<Func<T, bool>>> expressions = new List<Expression<Func<T, bool>>>();
            foreach (var filter in filterRequests)
            {
                Expression<Func<T, bool>>? expression = filter.ToExpression<T>(properties, param);

                if (expression != null) expressions.Add(expression);
            }

            if (!expressions.Any())
            {
                return null;
            }

            var result = expressions.First();
            for (int i = 1; i < expressions.Count; i++)
            {
                var exp = expressions[i];

                var leftVisitor = new ReplaceExpressionVisitor(result.Parameters[0], param);
                var left = leftVisitor.Visit(result.Body)!;
                
                var rightVisitor = new ReplaceExpressionVisitor(exp.Parameters[0], param);
                var right = rightVisitor.Visit(exp.Body)!;

                result = ExpressionHelper.CreateAndExpression<T>(param, left, right);
            }

            return result;
        }
    }
}