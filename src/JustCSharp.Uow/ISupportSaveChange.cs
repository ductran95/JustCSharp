using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Uow
{
    public interface ISupportSaveChange
    {
        void SaveChanges();
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}