using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JustCSharp.Data.Constants;
using JustCSharp.Data.Requests;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.Utility.Extensions
{
    public static class FilterRequestExtensions
    {
        public static Expression<Func<T, bool>> ToExpression<T>(this IEnumerable<FilterRequest> filters)
        {
            if (filters == null || !filters.Any())
            {
                return null;
            }

            var properties = typeof(T).GetProperties(ReflectionConstants.SearchPropertyFlags);
            var param = Expression.Parameter(typeof(T), "p");
            
            List<Expression<Func<T, bool>>> expressions = new List<Expression<Func<T, bool>>>();
            foreach (var filter in filters)
            {
                var prop = properties.FirstOrDefault(x => x.Name == filter.Field);
                var propType = prop.PropertyType;

                Expression<Func<T, bool>> expression = null;

                if (filter.ValueDateTimeFrom != null || filter.ValueDateTimeTo != null)
                {
                    Expression<Func<T, bool>> expressionFrom = null;
                    Expression<Func<T, bool>> expressionTo = null;

                    if (filter.ValueDateTimeFrom != null)
                    {
                        expressionFrom =
                            ExpressionHelper.CreateGTEExpression<T>(param, prop.Name, filter.ValueDateTimeFrom.Value);
                    }
                    
                    if (filter.ValueDateTimeTo != null)
                    {
                        expressionTo =
                            ExpressionHelper.CreateLTEExpression<T>(param, prop.Name, filter.ValueDateTimeTo.Value);
                    }

                    if (expressionFrom != null && expressionTo != null)
                    {
                        var leftVisitor = new ReplaceExpressionVisitor(expressionFrom.Parameters[0], param);
                        var left = leftVisitor.Visit(expressionFrom.Body);
                
                        var rightVisitor = new ReplaceExpressionVisitor(expressionTo.Parameters[0], param);
                        var right = rightVisitor.Visit(expressionTo.Body);
                        
                        expression = ExpressionHelper.CreateAndExpression<T>(param, left, right);
                    }
                    else
                    {
                        expression = expressionFrom ?? expressionTo;
                    }
                }
                else if (filter.ValueNumberFrom != null || filter.ValueNumberTo != null)
                {
                    Expression<Func<T, bool>> expressionFrom = null;
                    Expression<Func<T, bool>> expressionTo = null;

                    if (filter.ValueNumberFrom != null)
                    {
                        expressionFrom =
                            ExpressionHelper.CreateGTEExpression<T>(param, prop.Name, filter.ValueNumberFrom.Value);
                    }
                    
                    if (filter.ValueNumberTo != null)
                    {
                        expressionTo =
                            ExpressionHelper.CreateLTEExpression<T>(param, prop.Name, filter.ValueNumberTo.Value);
                    }

                    if (expressionFrom != null && expressionTo != null)
                    {
                        var leftVisitor = new ReplaceExpressionVisitor(expressionFrom.Parameters[0], param);
                        var left = leftVisitor.Visit(expressionFrom.Body);
                
                        var rightVisitor = new ReplaceExpressionVisitor(expressionTo.Parameters[0], param);
                        var right = rightVisitor.Visit(expressionTo.Body);
                        
                        expression = ExpressionHelper.CreateAndExpression<T>(param, left, right);
                    }
                    else
                    {
                        expression = expressionFrom ?? expressionTo;
                    }
                }
                else if (filter.ValueBool != null)
                {
                    expression = ExpressionHelper.CreateEqualExpression<T>(param, prop.Name, filter.ValueBool.Value);
                }
                else if (filter.ValueList != null)
                {
                    expression = ExpressionHelper.CreateContainExpression<T>(param, prop.Name, filter.ValueList);
                }
                else
                {
                    expression = ExpressionHelper.CreateContainExpression<T>(param, prop.Name, filter.ValueString);
                }
                
                expressions.Add(expression);
            }

            var result = expressions.FirstOrDefault();
            for (int i = 1; i < expressions.Count; i++)
            {
                var exp = expressions[i];

                var leftVisitor = new ReplaceExpressionVisitor(result.Parameters[0], param);
                var left = leftVisitor.Visit(result.Body);
                
                var rightVisitor = new ReplaceExpressionVisitor(exp.Parameters[0], param);
                var right = rightVisitor.Visit(exp.Body);

                result = ExpressionHelper.CreateAndExpression<T>(param, left, right);
            }

            return result;
        }
    }
}