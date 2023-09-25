using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JustCSharp.Data.Linq;
using JustCSharp.Data.Requests;
using JustCSharp.Database.MongoDB.Model;
using MongoDB.Driver;

namespace JustCSharp.Database.MongoDB.Extensions;

public static class AggregateFluentExtensions
{
    public static IAggregateFluent<T> FilterBy<T>(this IAggregateFluent<T> query, IEnumerable<FilterRequest>? filters)
    {
        var filterRequests = filters as FilterRequest[] ?? filters?.ToArray();
        
        if (filterRequests == null || !filterRequests.Any())
        {
            return query;
        }

        var exp = filterRequests.ToExpression<T>();
        return query.Match(exp);
    }

    public static IAggregateFluent<T> OrderBy<T>(this IAggregateFluent<T> query, IEnumerable<SortRequest>? sorts)
    {
        var sortRequests = sorts as SortRequest[] ?? sorts?.ToArray();
        
        if (sortRequests == null || !sortRequests.Any())
        {
            return query;
        }

        IOrderedAggregateFluent<T> result;

        Expression<Func<T, object>>? firstSortExp = null;
        SortRequest firstSort = null!;
        int i = 0;
        do
        {
            firstSort = sortRequests[i];
            firstSortExp = firstSort.ToExpression<T>();
            i++;
        } while (firstSortExp != null && i < sortRequests.Count());

        if (firstSortExp == null)
        {
            return query;
        }
        
        if (firstSort.Asc)
        {
            result = query.SortBy(firstSortExp);
        }
        else
        {
            result = query.SortByDescending(firstSortExp);
        }

        for (; i < sortRequests.Count(); i++)
        {
            var sort = sortRequests[i];
            var sortExp = sort.ToExpression<T>();

            if (sortExp != null)
            {
                if (sort.Asc)
                {
                    result = result.ThenBy(sortExp);
                }
                else
                {
                    result = result.ThenByDescending(sortExp);
                }
            }
        }

        return result;
    }

    public static IAggregateFluent<T> PagingBy<T>(this IAggregateFluent<T> query, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Limit(pageSize);
    }
    
    public static IAggregateFluent<MongoFacetPagingResult<T>> FacetPagingBy<T>(
        this IAggregateFluent<T> query, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        var countPipeline = PipelineStageDefinitionBuilder.Count<T>();
        var skipPipeline = PipelineStageDefinitionBuilder.Skip<T>(skip);
        var limitPipeline = PipelineStageDefinitionBuilder.Limit<T>(pageSize);
        
        var countFacet = new AggregateFacet<T, AggregateCountResult>(
            nameof(MongoFacetPagingResult<T>.CountResult),
            PipelineDefinition<T, AggregateCountResult>.Create(
                new List<IPipelineStageDefinition>{countPipeline}));
        
        var pagingFacet = new AggregateFacet<T, T>(
            nameof(MongoFacetPagingResult<T>.Data),
            PipelineDefinition<T, T>.Create(
                new List<IPipelineStageDefinition>{skipPipeline, limitPipeline}));

        return query
            .Facet<T, MongoFacetPagingResult<T>>(countFacet, pagingFacet);
    }
}