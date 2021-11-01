using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Data.Entities;
using JustCSharp.Uow;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace JustCSharp.Database.MongoDB.Repositories
{
    public interface IMongoDbRepository<TEntity>: IRepository<TEntity> where TEntity : IEntity
    {
        IMongoDatabase GetDatabase();
        Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default);
        
        IMongoCollection<TEntity> GetCollection();
        Task<IMongoCollection<TEntity>> GetCollectionAsync(CancellationToken cancellationToken = default);
        
        IMongoQueryable<TEntity> GetMongoQueryable();
        Task<IMongoQueryable<TEntity>> GetMongoQueryableAsync(CancellationToken cancellationToken = default);
        
        IAggregateFluent<TEntity> GetAggregate();
        Task<IAggregateFluent<TEntity>> GetAggregateAsync(CancellationToken cancellationToken = default);
    }
}