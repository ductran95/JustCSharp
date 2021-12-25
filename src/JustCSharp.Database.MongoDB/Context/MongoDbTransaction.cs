using JustCSharp.Uow;
using MongoDB.Driver;

namespace JustCSharp.Database.MongoDB.Context
{
    public class MongoDbTransaction: ITransaction
    {
        public IClientSessionHandle SessionHandle { get; set; }

        public MongoDbTransaction(IClientSessionHandle sessionHandle)
        {
            SessionHandle = sessionHandle;
        }
    }
}