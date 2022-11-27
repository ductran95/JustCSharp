using System.Collections.Generic;

namespace JustCSharp.Data.Requests
{
    public class SearchRequest: IRequest
    {
        public List<FilterRequest>? Filters { get; set; }
        public List<SortRequest>? Sorts { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}