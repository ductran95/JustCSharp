namespace JustCSharp.MongoDB.Attribute
{
    public class MongoCollectionAttribute : System.Attribute
    {
        public string CollectionName { get; set; }

        public MongoCollectionAttribute()
        {
            
        }

        public MongoCollectionAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }
    }
}