using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JustCSharp.Data.Enums;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.Data.Requests
{
    public class FilterRequest: IRequest, IFilterRequest
    {
        public string Field { get; set; }
        public string ValueString { get; set; }
        public Guid? ValueGuid { get; set; }
        public DateTime? ValueDateTimeFrom { get; set; }
        public DateTime? ValueDateTimeTo { get; set; }
        public float? ValueNumberFrom { get; set; }
        public float? ValueNumberTo { get; set; }
        public bool? ValueBool { get; set; }
        public IEnumerable ValueList { get; set; }

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
    }
    
    public class FilterRequest<T>: IRequest, IFilterRequest
    {
        public string Field { get; set; } = default!;
        public FilterOperator Operator { get; set; }
        public T? Value { get; set; }

        public FilterRequest()
        {
        }

        public FilterRequest(string field, FilterOperator @operator, T? value)
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