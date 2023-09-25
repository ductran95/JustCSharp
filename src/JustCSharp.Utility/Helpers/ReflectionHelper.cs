using System;
using System.Diagnostics;
using System.Reflection;

namespace JustCSharp.Utility.Helpers
{
    public static class ReflectionHelper
    {
        public static readonly BindingFlags SearchPropertyFlags = BindingFlags.Instance | BindingFlags.Public |
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.FlattenHierarchy;

        public static readonly Type[] ConvertibleTypes = new Type[]
        {
            typeof(DBNull),
            typeof(bool),
            typeof(char),
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal),
            typeof(DateTime),
            typeof(string),
        };
        
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var givenTypeInfo = givenType.GetTypeInfo();

            if (givenTypeInfo.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            foreach (var interfaceType in givenTypeInfo.GetInterfaces())
            {
                if (interfaceType.GetTypeInfo().IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            if (givenTypeInfo.BaseType == null)
            {
                return false;
            }

            return IsAssignableToGenericType(givenTypeInfo.BaseType, genericType);
        }

        public static string GetCurrentMethodName()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame? stackFrame = stackTrace.GetFrame(1);

            if (stackFrame == null)
            {
                return string.Empty;
            }

            var method = stackFrame.GetMethod();

            if (method == null)
            {
                return string.Empty;
            }


            string methodName = method.Name;

            if (methodName == "MoveNext")
            {
                methodName = method.DeclaringType?.Name ?? string.Empty;
                var lessIndex = methodName.IndexOf('<') + 1;
                var greaterIndex = methodName.IndexOf('>');
                methodName = methodName[lessIndex..greaterIndex];
                string className = method.DeclaringType?.DeclaringType?.Name ?? string.Empty;
                return $"{className}.{methodName}";
            }
            else
            {
                string className = method.DeclaringType?.Name ?? string.Empty;
                return $"{className}.{methodName}";
            }
        }
    }
}