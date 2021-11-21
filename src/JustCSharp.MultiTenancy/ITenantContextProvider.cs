using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.MultiTenancy
{
    public interface ITenantContextProvider
    {
        ITenantContext TenantContext { get; }
        ITenantContext GetTenantContext();
        Task<ITenantContext> GetTenantContextAsync(CancellationToken cancellationToken = default);
    }
    
    public interface ITenantContextProviderOfT<TTenantContext>: ITenantContextProvider 
        where TTenantContext: class, ITenantContext
    {
        TTenantContext TenantContextOfT { get; }
        TTenantContext GetTenantContextOfT();
        Task<TTenantContext> GetTenantContextOfTAsync(CancellationToken cancellationToken = default);
    }
}