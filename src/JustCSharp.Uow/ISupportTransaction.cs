using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Uow
{
    public interface ISupportTransaction
    {
        ITransaction CurrentTransaction { get; }
        bool IsInTransaction();
        Task<bool> IsInTransactionAsync(CancellationToken cancellationToken = default);
        void BeginTransaction();
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        void CommitTransaction();
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        void RollbackTransaction();
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}