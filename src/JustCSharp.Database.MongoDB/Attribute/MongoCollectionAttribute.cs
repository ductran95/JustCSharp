namespace JustCSharp.Database.MongoDB.Attribute
{
    public class MongoCollectionAttribute : System.Attribute
    {
        public string CollectionName { get; set; }

        public MongoCollectionAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }
    }
}