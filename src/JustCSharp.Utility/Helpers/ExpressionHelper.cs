using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using JustCSharp.Utility.Extensions;

namespace JustCSharp.Utility.Helpers
{
    public static class ExpressionHelper
    {
        public static Expression<Func<T, bool>> CreateAndExpression<T>(ParameterExpression param, Expression exp1, Expression exp2)
        {
            var expressionAnd = Expression.AndAlso(exp1, exp2);
            return Expression.Lambda<Func<T, bool>>(expressionAnd, param);
        }
        
        public static Expression<Func<T, bool>> CreateAndExpression<T>(ParameterExpression param, Expression<Func<T, bool>> exp1, Expression<Func<T, bool>> exp2)
        {
            var expressionAnd = Expression.AndAlso(exp1, exp2);
            return Expression.Lambda<Func<T, bool>>(expressionAnd, param);
        }
            
        public static Expression<Func<T, bool>> CreateEqualExpression<T>(ParameterExpression param, string property,
            object data)
        {
            string[] properties = property.Split('.');
            Type propertyType = typeof(T);

            Expression propertyExp = param;

            foreach (var prop in properties)
            {
                PropertyInfo pi = propertyType.GetProperty(prop);
                propertyExp = Expression.Property(propertyExp, pi);
                propertyType = pi.PropertyType;
            }
            
            Expression dataExp = Expression.Constant(data, propertyType);
            
            return Expression.Lambda<Func<T, bool>>(Expression.Equal(propertyExp, dataExp), param);
        }
        
        public static Expression<Func<T, bool>> CreateGTEExpression<T>(ParameterExpression param, string property,
            object data)
        {
            string[] properties = property.Split('.');
            Type propertyType = typeof(T);

            Expression propertyExp = param;

            foreach (var prop in properties)
            {
                PropertyInfo pi = propertyType.GetProperty(prop);
                propertyExp = Expression.Property(propertyExp, pi);
                propertyType = pi.PropertyType;
            }
            
            Expression dataExp = Expression.Constant(data, propertyType);
            
            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(propertyExp, dataExp), param);
        }
        
        public static Expression<Func<T, bool>> CreateLTEExpression<T>(ParameterExpression param, string property,
            object data)
        {
            string[] properties = property.Split('.');
            Type propertyType = typeof(T);

            Expression propertyExp = param;

            foreach (var prop in properties)
            {
                PropertyInfo pi = propertyType.GetProperty(prop);
                propertyExp = Expression.Property(propertyExp, pi);
                propertyType = pi.PropertyType;
            }
            
            Expression dataExp = Expression.Constant(data, propertyType);
            
            return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(propertyExp, dataExp), param);
        }
        
        public static Expression<Func<T, bool>> CreateContainExpression<T>(ParameterExpression param, string property,
            string data)
        {
            MethodInfo containMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
            
            string[] properties = property.Split('.');
            Type propertyType = typeof(T);

            Expression propertyExp = param;

            foreach (var prop in properties)
            {
                PropertyInfo pi = propertyType.GetProperty(prop);
                propertyExp = Expression.Property(propertyExp, pi);
                propertyType = pi.PropertyType;
            }
            
            Expression dataExp = Expression.Constant(data, propertyType);
            
            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.Lambda<Func<T, bool>>(Expression.Call(propertyExp, containMethod, dataExp), param);
        }
        
        public static Expression<Func<T, bool>> CreateContainExpression<T>(ParameterExpression param, string property,
            ArrayList data)
        {
            string[] properties = property.Split('.');
            Type propertyType = typeof(T);

            Expression propertyExp = param;

            foreach (var prop in properties)
            {
                PropertyInfo pi = propertyType.GetProperty(prop);
                propertyExp = Expression.Property(propertyExp, pi);
                propertyType = pi.PropertyType;
            }
            
            var listType = typeof(List<>).MakeGenericType(propertyType);

            var listData = data.Cast<JsonElement>().Select(x => x.ToObject(propertyType)).ToList();
            MethodInfo containMethod = listType.GetMethod("Contains", new[] {propertyType});

            object listDataWithType = listData.ChangeType(propertyType);
            var dataExp = Expression.Constant(listDataWithType);
            
            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.Lambda<Func<T, bool>>(Expression.Call(dataExp, containMethod, propertyExp), param);
        }
    }
}