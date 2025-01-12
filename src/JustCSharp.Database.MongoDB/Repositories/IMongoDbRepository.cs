using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Data.Entities;
using JustCSharp.Uow;
using MongoDB.Driver;

namespace JustCSharp.Database.MongoDB.Repositories
{
    public interface IMongoDbRepository<TEntity>: IRepository<TEntity> 
        where TEntity : class, IEntity
    {
        IMongoDatabase GetDatabase();
        Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default);
        
        IMongoCollection<TEntity> GetCollection();
        Task<IMongoCollection<TEntity>> GetCollectionAsync(CancellationToken cancellationToken = default);
        
        IQueryable<TEntity> GetMongoQueryable();
        Task<IQueryable<TEntity>> GetMongoQueryableAsync(CancellationToken cancellationToken = default);
        
        IAggregateFluent<TEntity> GetAggregate();
        Task<IAggregateFluent<TEntity>> GetAggregateAsync(CancellationToken cancellationToken = default);
    }
}