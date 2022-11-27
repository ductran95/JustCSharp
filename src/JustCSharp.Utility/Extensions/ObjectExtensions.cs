using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JustCSharp.Utility.Extensions
{
    public static class ObjectExtensions
    {
        private static readonly Dictionary<Type, object?> DefaultTypeValues = new();

        public static bool ContainsProperty(this object source, string propName)
        {
            var prop = source.GetType().GetProperty(propName);
            return prop != null;
        }

        public static string GetGenericTypeName(this object source)
        {
            return source.GetType().GetGenericTypeName();
        }

        public static T? GetPropertyValue<T>(this object? source, string propName)
        {
            if (source == null)
            {
                return default(T);
            }

            if (source is JsonDocument jsonDocument)
            {
                JsonElement root = jsonDocument.RootElement;
                var jsonProp = root.GetProperty(propName);
                return (T?)jsonProp.ToObject(typeof(T));
            }

            var prop = source.GetType().GetProperty(propName);
            var field = source.GetType().GetField(propName);

            if (prop == null && field == null)
            {
                return default(T);
            }

            var value = prop?.GetValue(source) ?? field?.GetValue(source);

            return value != null ? (T)value : default(T);
        }

        public static object? GetPropertyValue(this object? source, string propName)
        {
            if (source == null)
            {
                return null;
            }

            if (source is JsonDocument jsonDocument)
            {
                JsonElement root = jsonDocument.RootElement;
                var jsonProp = root.GetProperty(propName);
                return jsonProp;
            }

            var prop = source.GetType().GetProperty(propName);
            var field = source.GetType().GetField(propName);

            if (prop == null && field == null)
            {
                return null;
            }

            return prop?.GetValue(source) ?? field?.GetValue(source);
        }

        public static object? ChangeType(this object @object, Type type)
        {
            object? result = null;
            var nonNullType = Nullable.GetUnderlyingType(type);

            if (nonNullType != null)
            {
                type = nonNullType;
            }

            if (type.IsConvertible())
            {
                result = Convert.ChangeType(@object, type);
            }
            else
            {
                if (type.IsEnum)
                {
                    result = Convert.ChangeType(@object, typeof(int));
                    //result = Enum.ToObject(type, result);
                }
            }

            return result;
        }

        public static object ToNullable(this object @object)
        {
            var type = @object.GetType();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return (bool?)@object;

                case TypeCode.Byte:
                    return (byte?)@object;

                case TypeCode.SByte:
                    return (sbyte?)@object;

                case TypeCode.Int16:
                    return (short?)@object;

                case TypeCode.UInt16:
                    return (ushort?)@object;

                case TypeCode.Int32:
                    return (int?)@object;

                case TypeCode.UInt32:
                    return (uint?)@object;

                case TypeCode.Int64:
                    return (long?)@object;

                case TypeCode.UInt64:
                    return (ulong?)@object;

                case TypeCode.Single:
                    return (float?)@object;

                case TypeCode.Double:
                    return (double?)@object;

                case TypeCode.Decimal:
                    return (decimal?)@object;

                case TypeCode.DateTime:
                    return (DateTime?)@object;

                case TypeCode.String:
                    return @object;

                default:
                    if (type == typeof(Guid))
                    {
                        return (bool?)@object;
                    }

                    throw new NotImplementedException();
            }
        }

        public static object? GetDefaultValue(this Type type)
        {
            if (DefaultTypeValues.ContainsKey(type))
            {
                return DefaultTypeValues[type];
            }

            object? defaultValue;

            if (type.IsValueType)
            {
                defaultValue = Activator.CreateInstance(type);
            }
            else
            {
                defaultValue = null;
            }

            DefaultTypeValues[type] = defaultValue;
            return defaultValue;
        }
    }
}