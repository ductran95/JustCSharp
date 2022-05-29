using System.Collections.Generic;

namespace JustCSharp.Data.Results;

public class Result<T>
{
    public T Data { get; set; }
    public bool Success { get; set; }
    public IEnumerable<Error> Errors { get; set; }
}
