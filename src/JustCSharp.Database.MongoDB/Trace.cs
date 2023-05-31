using System.Diagnostics;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.Database.MongoDB;

internal class Trace
{
    private static readonly string TraceName = typeof(Trace).Namespace ?? "JustCSharp.Database.MongoDB";

    internal static ActivitySource ActivitySource { get; } =
        new(TraceName, AssemblyHelper.GetAssemblyVersion(typeof(Trace)));
}