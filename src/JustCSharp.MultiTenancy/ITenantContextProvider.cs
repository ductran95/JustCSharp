using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Authentication
{
    public interface ITenantContextProvider
    {
        ITenantContext TenantContext { get; }
        ITenantContext GetTenantContext();
        Task<ITenantContext> GetTenantContextAsync(CancellationToken cancellationToken = default);
    }
    
    public interface ITenantContextProviderOfT<TAuthContext>: ITenantContextProvider 
        where TAuthContext: class, ITenantContext
    {
        TAuthContext AuthContextOfT { get; }
        TAuthContext GetTenantContextOfT();
        Task<TAuthContext> GetTenantContextOfTAsync(CancellationToken cancellationToken = default);
    }
}