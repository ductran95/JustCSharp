using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Database.MongoDB.Model;
using MongoDB.Driver;

namespace JustCSharp.Database.MongoDB.Context
{
    public interface IMongoDbContext
    {
        Dictionary<Type, IMongoEntityModel> EntityModels { get; }
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
        IClientSessionHandle SessionHandle { get; }
        
        IMongoCollection<T> Collection<T>();
        void CheckStateAndConnect();
        Task CheckStateAndConnectAsync(CancellationToken cancellationToken = default);
    }
}