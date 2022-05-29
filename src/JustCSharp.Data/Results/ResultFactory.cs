using System.Collections.Generic;

namespace JustCSharp.Data.Results;

public static class ResultFactory
{
    public static Result<T> Single<T>(T data)
    {
        return new Result<T>
        {
            Success = true,
            Data = data
        };
    }
    
    public static Result<T> Error<T>(T data, params Error[] errors)
    {
        return new Result<T>
        {
            Success = true,
            Errors = errors,
            Data = data
        };
    }
    
    public static PagingResult<T> Paging<T>(PagedData<T> data)
    {
        return new PagingResult<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ListingResult<T> Listing<T>(IEnumerable<T> data)
    {
        return new ListingResult<T>
        {
            Success = true,
            Data = data
        };
    }
}