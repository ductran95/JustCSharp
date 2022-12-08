using System.Collections.Generic;
using JustCSharp.Data.Enums;

namespace JustCSharp.Data.Constants;

public static class FilterOperators
{
    public static readonly Dictionary<FilterValueType, List<FilterOperator>> TypeOperationsMapping = new()
    {
        {
            FilterValueType.Bool, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
            }
        },
        {
            FilterValueType.Byte, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
                FilterOperator.GreaterThan,
                FilterOperator.GreaterThanOrEqual,
                FilterOperator.LessThan,
                FilterOperator.LessThanOrEqual,
            }
        },
        {
            FilterValueType.Int32, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
                FilterOperator.GreaterThan,
                FilterOperator.GreaterThanOrEqual,
                FilterOperator.LessThan,
                FilterOperator.LessThanOrEqual,
            }
        },
        {
            FilterValueType.Int64, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
                FilterOperator.GreaterThan,
                FilterOperator.GreaterThanOrEqual,
                FilterOperator.LessThan,
                FilterOperator.LessThanOrEqual,
            }
        },
        {
            FilterValueType.Float, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
                FilterOperator.GreaterThan,
                FilterOperator.GreaterThanOrEqual,
                FilterOperator.LessThan,
                FilterOperator.LessThanOrEqual,
            }
        },
        {
            FilterValueType.Double, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
                FilterOperator.GreaterThan,
                FilterOperator.GreaterThanOrEqual,
                FilterOperator.LessThan,
                FilterOperator.LessThanOrEqual,
            }
        },
        {
            FilterValueType.Decimal, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
                FilterOperator.GreaterThan,
                FilterOperator.GreaterThanOrEqual,
                FilterOperator.LessThan,
                FilterOperator.LessThanOrEqual,
            }
        },
        {
            FilterValueType.String, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
                FilterOperator.Contain,
                FilterOperator.NotContain,
                FilterOperator.StartWith,
                FilterOperator.NotStartWith,
                FilterOperator.EndWith,
                FilterOperator.NotEndWith,
            }
        },
        {
            FilterValueType.Guid, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
            }
        },
        {
            FilterValueType.DateTime, new List<FilterOperator>()
            {
                FilterOperator.Equal,
                FilterOperator.NotEqual,
                FilterOperator.GreaterThan,
                FilterOperator.GreaterThanOrEqual,
                FilterOperator.LessThan,
                FilterOperator.LessThanOrEqual,
            }
        }
    };
}