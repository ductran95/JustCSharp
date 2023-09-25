using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JustCSharp.Data.Enums;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.Data.Requests
{
    public class FilterRequest: IRequest, IFilterRequest
    {
        public string? Field { get; set; }
        public string? ValueString { get; set; }
        public Guid? ValueGuid { get; set; }
        public DateTime? ValueDateTimeFrom { get; set; }
        public DateTime? ValueDateTimeTo { get; set; }
        public float? ValueNumberFrom { get; set; }
        public float? ValueNumberTo { get; set; }
        public bool? ValueBool { get; set; }
        public IEnumerable? ValueList { get; set; }

        private List<IFilterRequest> ToFiltersWithType()
        {
            var filters = new List<IFilterRequest>();
            
            if (!string.IsNullOrEmpty(ValueString))
            {
                filters.Add(new FilterRequest<string?>(Field, FilterOperator.Contain, ValueString));
            }
            
            if (ValueGuid != null)
            {
                filters.Add(new FilterRequest<Guid?>(Field, FilterOperator.Equal, ValueGuid));
            }
            
            if (ValueBool != null)
            {
                filters.Add(new FilterRequest<bool?>(Field, FilterOperator.Equal, ValueBool));
            }
            
            if (ValueDateTimeFrom != null)
            {
                filters.Add(new FilterRequest<DateTime?>(Field, FilterOperator.GreaterThanOrEqual, ValueDateTimeFrom));
            }
            
            if (ValueDateTimeTo != null)
            {
                filters.Add(new FilterRequest<DateTime?>(Field, FilterOperator.LessThanOrEqual, ValueDateTimeTo));
            }
            
            if (ValueNumberFrom != null)
            {
                filters.Add(new FilterRequest<float?>(Field, FilterOperator.GreaterThanOrEqual, ValueNumberFrom));
            }
            
            if (ValueNumberTo != null)
            {
                filters.Add(new FilterRequest<float?>(Field, FilterOperator.LessThanOrEqual, ValueNumberTo));
            }


            return filters;
        }
        
        public Expression<Func<TEntity, bool>> ToExpression<TEntity>()
        {
            var typeFilters = ToFiltersWithType();

            Expression<Func<TEntity, bool>> expression = null!;
            
            if (typeFilters.Any())
            {
                foreach (var typeFilter in typeFilters)
                {
                    var itemExp = typeFilter.ToExpression<TEntity>();
                    if (expression == null)
                    {
                        expression = itemExp;
                    }
                    else
                    {
                        expression = ExpressionHelper.CreateAndExpression(expression.Parameters[0], expression, itemExp);
                    }
                }
            }
            else
            {
                expression = entity => true;
            }
            
            return expression;
        }
        
        public Expression<Func<TEntity, bool>>? ToExpression<TEntity>(PropertyInfo[] properties, ParameterExpression param)
        {
            var prop = properties.FirstOrDefault(x => x.Name == Field);
            Expression<Func<TEntity, bool>>? expression = null;

            if (prop == null)
            {
                return expression;
            }
            
            if (ValueDateTimeFrom != null || ValueDateTimeTo != null)
            {
                Expression<Func<TEntity, bool>>? expressionFrom = null;
                Expression<Func<TEntity, bool>>? expressionTo = null;

                if (ValueDateTimeFrom != null)
                {
                    expressionFrom =
                        ExpressionHelper.CreateGTEExpression<TEntity>(param, prop.Name, ValueDateTimeFrom.Value);
                }
                
                if (ValueDateTimeTo != null)
                {
                    expressionTo =
                        ExpressionHelper.CreateLTEExpression<TEntity>(param, prop.Name, ValueDateTimeTo.Value);
                }

                if (expressionFrom != null && expressionTo != null)
                {
                    var leftVisitor = new ReplaceExpressionVisitor(expressionFrom.Parameters[0], param);
                    var left = leftVisitor.Visit(expressionFrom.Body)!;
            
                    var rightVisitor = new ReplaceExpressionVisitor(expressionTo.Parameters[0], param);
                    var right = rightVisitor.Visit(expressionTo.Body)!;
                    
                    expression = ExpressionHelper.CreateAndExpression<TEntity>(param, left, right);
                }
                else
                {
                    expression = expressionFrom ?? expressionTo;
                }
            }
            else if (ValueNumberFrom != null || ValueNumberTo != null)
            {
                Expression<Func<TEntity, bool>>? expressionFrom = null;
                Expression<Func<TEntity, bool>>? expressionTo = null;

                if (ValueNumberFrom != null)
                {
                    expressionFrom =
                        ExpressionHelper.CreateGTEExpression<TEntity>(param, prop.Name, ValueNumberFrom.Value);
                }
                
                if (ValueNumberTo != null)
                {
                    expressionTo =
                        ExpressionHelper.CreateLTEExpression<TEntity>(param, prop.Name, ValueNumberTo.Value);
                }

                if (expressionFrom != null && expressionTo != null)
                {
                    var leftVisitor = new ReplaceExpressionVisitor(expressionFrom.Parameters[0], param);
                    var left = leftVisitor.Visit(expressionFrom.Body)!;
            
                    var rightVisitor = new ReplaceExpressionVisitor(expressionTo.Parameters[0], param);
                    var right = rightVisitor.Visit(expressionTo.Body)!;
                    
                    expression = ExpressionHelper.CreateAndExpression<TEntity>(param, left, right);
                }
                else
                {
                    expression = expressionFrom ?? expressionTo;
                }
            }
            else if (ValueBool != null)
            {
                expression = ExpressionHelper.CreateEqualExpression<TEntity>(param, prop.Name, ValueBool.Value);
            }
            else if (ValueList != null)
            {
                expression = ExpressionHelper.CreateContainExpression<TEntity>(param, prop.Name, ValueList);
            }
            else
            {
                expression = ExpressionHelper.CreateContainExpression<TEntity>(param, prop.Name, ValueString, StringComparison.InvariantCultureIgnoreCase);
            }

            return expression;
        }
    }
    
    public class FilterRequest<T>: IRequest, IFilterRequest
    {
        public string? Field { get; set; }
        public FilterOperator Operator { get; set; }
        public T? Value { get; set; }

        public FilterRequest()
        {
        }

        public FilterRequest(string? field, FilterOperator @operator, T? value)
        {
            Field = field;
            Operator = @operator;
            Value = value;
        }

        public Expression<Func<TEntity, bool>> ToExpression<TEntity>()
        {
            throw new NotImplementedException();
        }
    }
}