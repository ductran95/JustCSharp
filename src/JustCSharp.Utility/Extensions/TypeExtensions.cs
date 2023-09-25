using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.Utility.Extensions
{
    public static class TypeExtensions
    {
        public static bool ContainsProperty(this Type type, string propName)
        {
            var captionFirstLetter = char.ToUpper(propName[0]) + propName.Substring(1);
            var prop = type.GetProperty(captionFirstLetter);
            return prop != null;
        }

        public static string GetGenericTypeName(this Type type)
        {
            string typeName;

            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        public static bool IsConvertible(this Type type)
        {
            return ReflectionHelper.ConvertibleTypes.Contains(type);
        }

        public static Type GetItemType(this Type enumerableType)
        {
            var genericTypes = enumerableType.GetGenericArguments();

            // List
            if (genericTypes.Any())
            {
                return genericTypes.Last(); // get last for select query
            }
            else
            {
                // Array
                var elementType = enumerableType.GetElementType();
                if (elementType != null)
                {
                    return elementType;
                }
            }

            throw new NotImplementedException();
        }

        public static bool IsIEnumerable(this Type type)
        {
            var isImplementIEnumerable = !type.Equals(typeof(string)) && type.GetInterfaces()
                .Any(x => x.IsGenericType && x.GetInterfaces().Contains(typeof(IEnumerable)));
            return isImplementIEnumerable;
        }
        
        public static IEnumerable<TypeInfo> GetConstructibleTypes(this Assembly assembly)
            => assembly.GetLoadableDefinedTypes().Where(
                t => !t.IsAbstract
                     && !t.IsGenericTypeDefinition);
        
        public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo!);
            }
        }
    }
}