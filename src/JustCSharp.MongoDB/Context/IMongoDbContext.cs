using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JustCSharp.MongoDB.Model;
using MongoDB.Driver;

namespace JustCSharp.MongoDB.Context
{
    public interface IMongoDbContext
    {
        Dictionary<Type, IMongoEntityModel> ModelCache { get; }
        
        IMongoClient Client { get; }

        IMongoDatabase Database { get; }

        IMongoCollection<T> Collection<T>();

        IClientSessionHandle SessionHandle { get; }
        void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle sessionHandle);
        void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle sessionHandle, Dictionary<Type, IMongoEntityModel> modelCache);
    }
}