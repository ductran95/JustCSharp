using System.Collections.Generic;
using System.Linq;
using JustCSharp.Data.Requests;

namespace JustCSharp.Data.Linq
{
    public static class SearchRequestExtensions
    {
        public static void UpdateFilter(this SearchRequest request, FilterRequest filter)
        {
            if (request.Filters == null)
            {
                request.Filters = new List<FilterRequest>();
            }

            var oldFilter = request.Filters.FirstOrDefault(x =>
                x.Field.ToLower() == filter.Field.ToLower());

            if (oldFilter != null)
            {
                oldFilter.Field = filter.Field;
                oldFilter.ValueString = filter.ValueString;
                oldFilter.ValueGuid = filter.ValueGuid;
                oldFilter.ValueDateTimeFrom = filter.ValueDateTimeFrom;
                oldFilter.ValueDateTimeTo = filter.ValueDateTimeTo;
                oldFilter.ValueNumberFrom = filter.ValueNumberFrom;
                oldFilter.ValueNumberTo = filter.ValueNumberTo;
                oldFilter.ValueBool = filter.ValueBool;
                oldFilter.ValueList = filter.ValueList;
            }
            else
            {
                request.Filters.Add(filter);
            }
        }
    }
}