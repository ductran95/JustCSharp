using MongoDB.Driver;

namespace JustCSharp.MongoDB.Context
{
    public interface IMongoDbContext
    {
        IMongoClient Client { get; }

        IMongoDatabase Database { get; }

        IMongoCollection<T> Collection<T>();

        IClientSessionHandle SessionHandle { get; }
        void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle sessionHandle);
    }
}