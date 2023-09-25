using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace JustCSharp.Database.MongoDB.Model;

public class MongoFacetPagingResult<TData>
{
    public long Count => CountResult?.FirstOrDefault()?.Count ?? 0;
    public MongoFacetCountResult[]? CountResult { get; set; }
    public List<TData>? Data { get; set; }
}

public class MongoFacetCountResult
{
    [BsonElement("count")]
    public long Count { get; set; }
}