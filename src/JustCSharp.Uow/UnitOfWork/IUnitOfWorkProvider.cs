using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Uow.UnitOfWork
{
    public interface IUnitOfWorkProvider
    {
        IUnitOfWork GetUnitOfWork();
        Task<IUnitOfWork> GetUnitOfWorkAsync(CancellationToken cancellationToken = default);
    }
}