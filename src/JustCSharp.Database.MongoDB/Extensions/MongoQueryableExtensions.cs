using System.Collections.Generic;
using System.Linq;
using JustCSharp.Data.Linq;
using JustCSharp.Data.Requests;
using JustCSharp.Utility.Extensions;
using MongoDB.Driver.Linq;

namespace JustCSharp.Database.MongoDB.Extensions
{
    public static class MongoQueryableExtensions
    {
        public static IMongoQueryable<T> FilterBy<T>(this IMongoQueryable<T> query, IEnumerable<FilterRequest> filters)
        {
            if (filters == null || !filters.Any())
            {
                return query;
            }
            
            var exp = filters.ToExpression<T>();
            return query.Where(exp);
        }

        public static IMongoQueryable<T> SortBy<T>(this IMongoQueryable<T> query, IEnumerable<SortRequest> sorts)
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
            
            return result as IMongoQueryable<T>;
        }
        
        public static IMongoQueryable<T> PagingBy<T>(this IMongoQueryable<T> query, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            
            return query.Skip(skip).Take(pageSize);
        }
    }
}