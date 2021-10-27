using System;

namespace JustCSharp.MongoDB.Model
{
    public class MongoEntityModel
    {
        public Type EntityType { get; internal set; }

        public string CollectionName { get;internal set; }
    }
}