using System.Collections.Generic;
using System.Linq;
using JustCSharp.Data.Requests;

namespace JustCSharp.Utility.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> FilterBy<T>(this IQueryable<T> query, IEnumerable<FilterRequest> filters)
        {
            if (filters == null || !filters.Any())
            {
                return query;
            }
            
            var exp = filters.ToExpression<T>();
            return query.Where(exp);
        }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> query, IEnumerable<SortRequest> sorts)
        {
            if (sorts == null || !sorts.Any())
            {
                return query;
            }
            
            var firstSort = sorts.FirstOrDefault();
            IOrderedQueryable<T> result = null;

            // ReSharper disable once PossibleNullReferenceException
            if (firstSort.Asc)
            {
                result = query.OrderBy(firstSort.Field);
            }
            else
            {
                result = query.OrderByDescending(firstSort.Field);
            }

            for (int i = 1; i < sorts.Count(); i++)
            {
                var sort = sorts.ElementAt(i);
                if (sort.Asc)
                {
                    result = result.ThenBy(sort.Field);
                }
                else
                {
                    result = result.ThenByDescending(sort.Field);
                }
            }
            
            return result;
        }
        
        public static IQueryable<T> PagingBy<T>(this IQueryable<T> query, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            
            return query.Skip(skip).Take(pageSize);
        }
    }
}