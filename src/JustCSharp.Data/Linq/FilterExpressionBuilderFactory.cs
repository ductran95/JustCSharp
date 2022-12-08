using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JustCSharp.Data.Requests;

namespace JustCSharp.Data.Linq;

public static class FilterExpressionBuilderFactory
{
    private static readonly Type _interfaceType = typeof(IFilterRequest);
    private static readonly List<IFilterExpressionBuilder> _filterExpressionBuilders = new();

    static FilterExpressionBuilderFactory()
    {
        ScanFilterExpressionBuilder<IFilterExpressionBuilder>();
    }
    
    public static void ScanFilterExpressionBuilder(Assembly assembly)
    {
        var implementationTypes = assembly.GetTypes()
            .Where(x => x.IsClass && !x.IsAbstract && _interfaceType.IsAssignableFrom(x));

        foreach (var implementationType in implementationTypes)
        {
            _filterExpressionBuilders.Add(Activator.CreateInstance(implementationType) as IFilterExpressionBuilder);
        }
    }
    
    public static void ScanFilterExpressionBuilder<TType>()
    {
        ScanFilterExpressionBuilder(typeof(TType).Assembly);
    }

    public static IFilterExpressionBuilder CreateBuilder(IFilterRequest filterRequest)
    {
        return _filterExpressionBuilders.First(x => x.IsSatisfied(filterRequest));
    }
}