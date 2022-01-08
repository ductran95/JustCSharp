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
    
    public interface ITenantContextProvider<TTenantContext>: ITenantContextProvider 
        where TTenantContext: class, ITenantContext
    {
        new TTenantContext TenantContext { get; }
        new TTenantContext GetTenantContext();
        new Task<TTenantContext> GetTenantContextAsync(CancellationToken cancellationToken = default);
    }
}