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
    
    public interface IUnitOfWorkProvider<TUnitOfWork>: IUnitOfWorkProvider 
        where TUnitOfWork: class, IUnitOfWork
    {
        new TUnitOfWork UnitOfWork { get; }
        new TUnitOfWork GetUnitOfWork();
        new Task<TUnitOfWork> GetUnitOfWorkAsync(CancellationToken cancellationToken = default);
    }
}