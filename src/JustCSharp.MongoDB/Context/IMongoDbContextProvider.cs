using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.MongoDB.Context
{
    public interface IMongoDbContextProvider
    {
        TDbContext GetDbContext<TDbContext>() where TDbContext: IMongoDbContext;
        Task<TDbContext> GetDbContextAsync<TDbContext>(CancellationToken cancellationToken = default) where TDbContext: IMongoDbContext;
    }
}