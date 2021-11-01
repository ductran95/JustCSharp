using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Uow.UnitOfWork
{
    public interface IUnitOfWorkProvider
    {
        IUnitOfWork UnitOfWork { get; }
        IUnitOfWork GetUnitOfWork();
        Task<IUnitOfWork> GetUnitOfWorkAsync(CancellationToken cancellationToken = default);
    }
    
    public interface IUnitOfWorkProviderOfT<TUnitOfWork>: IUnitOfWorkProvider 
        where TUnitOfWork: class, IUnitOfWork
    {
        TUnitOfWork UnitOfWorkOfT { get; }
        TUnitOfWork GetUnitOfWorkOfT();
        Task<TUnitOfWork> GetUnitOfWorkOfTAsync(CancellationToken cancellationToken = default);
    }
}