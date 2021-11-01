using System;
using MongoDB.Bson.Serialization;

namespace JustCSharp.MongoDB.Model
{
    public interface IMongoEntityModel
    {
        public Type EntityType { get; set; }

        public string CollectionName { get; set; }

        public BsonClassMap BsonMap { get; }
    }
    
    public interface IMongoEntityModel<TEntity>
    {
        public Type EntityType { get; set; }

        public string CollectionName { get; set; }

        public BsonClassMap<TEntity> BsonMap { get; }
    }
}