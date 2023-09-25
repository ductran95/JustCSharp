using System.Collections.Generic;
using System.Linq;
using JustCSharp.Data.Requests;
using JustCSharp.Utility.Extensions;

namespace JustCSharp.Data.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> FilterBy<T>(this IQueryable<T> query, IEnumerable<FilterRequest>? filters)
        {
            var filterRequests = filters as FilterRequest[] ?? filters?.ToArray();
            if (filterRequests == null || !filterRequests.Any())
            {
                return query;
            }
            
            var exp = filterRequests.ToExpression<T>();
            if (exp != null)
            {
                return query.Where(exp);
            }
            else
            {
                return query;
            }
        }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> query, IEnumerable<SortRequest>? sorts)
        {
            var sortRequests = sorts as SortRequest[] ?? sorts?.ToArray();
            if (sortRequests == null || !sortRequests.Any())
            {
                return query;
            }
            
            var firstSort = sortRequests.First();
            IOrderedQueryable<T>? result = null;

            // ReSharper disable once PossibleNullReferenceException
            if (firstSort.Asc)
            {
                result = query.OrderBy(firstSort.Field);
            }
            else
            {
                result = query.OrderByDescending(firstSort.Field);
            }

            for (int i = 1; i < sortRequests.Count(); i++)
            {
                var sort = sortRequests.ElementAt(i);
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