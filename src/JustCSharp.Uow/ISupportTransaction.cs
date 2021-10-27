using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Uow
{
    public interface ISupportTransaction
    {
        bool IsInTransaction();
        Task<bool> IsInTransactionAsync(CancellationToken cancellationToken = default);
        void Begin();
        Task BeginAsync(CancellationToken cancellationToken = default);
        void Commit();
        Task CommitAsync(CancellationToken cancellationToken = default);
        void Rollback();
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}