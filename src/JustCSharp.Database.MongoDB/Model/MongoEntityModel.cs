using System;
using MongoDB.Bson.Serialization;

namespace JustCSharp.Database.MongoDB.Model
{
    public sealed class MongoEntityModel<TEntity>: IMongoEntityModel, IMongoEntityModel<TEntity>
    {
        private readonly BsonClassMap<TEntity> _bsonClassMap;
        
        public Type EntityType { get; set; }
        public string CollectionName { get; set; }
        public BsonClassMap<TEntity> BsonMap => _bsonClassMap;

        BsonClassMap IMongoEntityModel.BsonMap => _bsonClassMap;
        
        public MongoEntityModel()
        {
            EntityType = typeof(TEntity);
            _bsonClassMap = new BsonClassMap<TEntity>();
            ConfigureDefaultMap();
        }

        private void ConfigureDefaultMap()
        {
            _bsonClassMap.AutoMap();
        }
    }
}