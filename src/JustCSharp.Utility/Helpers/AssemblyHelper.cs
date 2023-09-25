using System;
using System.Linq;
using System.Reflection;

namespace JustCSharp.Utility.Helpers;

public static class AssemblyHelper
{
    public static string JustCSharpAssemblyVersion => GetAssemblyVersion(typeof(AssemblyHelper));
    public static string ExecutingAssemblyVersion => GetAssemblyVersion(Assembly.GetCallingAssembly());

    public static string GetAssemblyVersion(Assembly assembly)
    {
        try
        {
            string versionString = assembly
                    .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                    .First()
                    .InformationalVersion;

            // Informational version will be something like 1.1.0-beta2+a25741030f05c60c85be102ce7c33f3899290d49.
            // Ignoring part after '+' if it is present.
            string shortVersion = versionString.Split('+')[0];

            return shortVersion;
        }
        catch (Exception)
        {
            var version = assembly.GetName().Version ?? new Version(1,0,0);
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    public static string GetAssemblyVersion(Type type)
    {
        return GetAssemblyVersion(type.Assembly);
    }

    public static string GetAssemblyVersion<T>()
    {
        return GetAssemblyVersion(typeof(T));
    }
}