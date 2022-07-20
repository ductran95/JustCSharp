using System;
using System.Reflection;

namespace JustCSharp.Utility.Helpers;

public static class AssemblyHelper
{
    public static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
    public static readonly string AssemblyVersionString = $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";
}