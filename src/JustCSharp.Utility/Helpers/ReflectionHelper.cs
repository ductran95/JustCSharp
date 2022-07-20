using System;
using System.Diagnostics;
using System.Reflection;

namespace JustCSharp.Utility.Helpers
{
    public static class ReflectionHelper
    {
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var givenTypeInfo = givenType.GetTypeInfo();

            if (givenTypeInfo.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            foreach (var interfaceType in givenTypeInfo.GetInterfaces())
            {
                if (interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == genericType)
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
            StackFrame stackFrame = stackTrace.GetFrame(1);

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
                methodName = method.DeclaringType?.Name;
                var lessIndex = methodName.IndexOf('<') + 1;
                var greaterIndex = methodName.IndexOf('>');
                methodName = methodName[lessIndex..greaterIndex];
                string className = method.DeclaringType?.DeclaringType?.Name;
                return $"{className}.{methodName}"; 
            }
            else
            {
                string className = method.DeclaringType?.Name;
                return $"{className}.{methodName}";
            }
        }
    }
}