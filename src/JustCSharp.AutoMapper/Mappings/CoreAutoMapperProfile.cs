using System.Collections.Generic;
using AutoMapper;
using JustCSharp.Data;

namespace JustCSharp.AutoMapper.Mappings
{
    public class CoreAutoMapperProfile: Profile
    {
        public CoreAutoMapperProfile()
        {
            CreateMap(typeof(PagedData<>), typeof(PagedData<>)).ConvertUsing(typeof(PagingResponseConverter<,>));
        }
    }
    
    public class PagingResponseConverter<TSource, TDestination> : ITypeConverter<PagedData<TSource>, PagedData<TDestination>>
    {
        public PagedData<TDestination> Convert(PagedData<TSource> source, PagedData<TDestination> destination, ResolutionContext context)
        {
            return new PagedData<TDestination>()
            {
                PageSize = source.PageSize,
                Total = source.Total,
                TotalPage = source.TotalPage,
                Data = context.Mapper.Map<IEnumerable<TDestination>>(source.Data)
            };
        }
    }
}