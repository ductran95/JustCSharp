using System;

namespace JustCSharp.Utility.Extensions
{
    public static class StringExtensions
    {
        public static object? ToType(this string? input, Type type)
        {
            bool isNullable = false;
            Type dataType = type;
            Type? nonNullType = Nullable.GetUnderlyingType(dataType);

            if (nonNullType != null)
            {
                isNullable = true;
                dataType = nonNullType;
            }

            if (dataType.IsClass)
            {
                isNullable = true;
            }

            object? result = null;

            if (input != null)
            {
                try
                {
                    switch (Type.GetTypeCode(dataType))
                    {
                        case TypeCode.Boolean:
                            result = bool.Parse(input);
                            break;

                        case TypeCode.Char:
                            result = char.Parse(input);
                            break;

                        case TypeCode.Byte:
                        case TypeCode.SByte:
                            result = byte.Parse(input);
                            break;

                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            result = int.Parse(input);
                            break;

                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            result = float.Parse(input);
                            break;

                        case TypeCode.String:
                            result = input;
                            break;

                        default:
                            if (dataType == typeof(Guid))
                            {
                                result = Guid.Parse(input);
                            }

                            break;
                    }
                }
                catch (Exception)
                {
                    throw new Exception($"Cannot convert \"{input}\" to type {dataType.Name}");
                }
            }

            if (!isNullable && result == null)
            {
                throw new Exception("Cannot convert null to non-nullable type");
            }

            return result;
        }

        public static object? ToType<T>(this string input)
        {
            return input.ToType(typeof(T));
        }
    }
}