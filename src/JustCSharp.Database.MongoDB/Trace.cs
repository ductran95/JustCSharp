using System.Diagnostics;
using JustCSharp.Utility.Helpers;

namespace JustCSharp.Database.MongoDB;

public class Trace
{
    public static readonly string TraceName = typeof(Trace).Namespace ?? "JustCSharp.Database.MongoDB";

    internal static ActivitySource ActivitySource { get; } =
        new(TraceName, AssemblyHelper.GetAssemblyVersion(typeof(Trace)));
}