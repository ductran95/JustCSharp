using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Database.MongoDB.Model;
using JustCSharp.Uow;
using MongoDB.Driver;

namespace JustCSharp.Database.MongoDB.Context
{
    public interface IMongoDbContext: IDatabase, ISupportTransaction
    {
        Dictionary<Type, IMongoEntityModel> EntityModels { get; }
        MongoDbContextOptions DbContextOptions { get; }
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
        IClientSessionHandle? SessionHandle { get; }
        bool IsConnected { get; }
        string DatabaseName { get; }
        MongoUrl MongoUrl { get; }

        IMongoCollection<T> Collection<T>();
        void CheckStateAndConnect();
        Task CheckStateAndConnectAsync(CancellationToken cancellationToken = default);
    }
}