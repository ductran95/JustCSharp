using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.DependencyInjection;
using JustCSharp.Core.Logging.Extensions;
using Microsoft.Extensions.Logging;

namespace JustCSharp.MultiTenancy
{
    public class TenantContextProvider: ITenantContextProvider
    {
        protected readonly ILazyServiceProvider _serviceProvider;

        protected ITenantContext _tenantContext;
        
        private ILogger Logger => _serviceProvider.GetLogger(typeof(TenantContextBase));

        public TenantContextProvider(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITenantContext TenantContext => GetTenantContext();
        
        public virtual ITenantContext GetTenantContext()
        {
            if (_tenantContext == null)
            {
                _tenantContext = CreateTenantContext();
            }
            return _tenantContext;
        }

        public virtual async Task<ITenantContext> GetTenantContextAsync(CancellationToken cancellationToken = default)
        {
            if (_tenantContext == null)
            {
                _tenantContext = await CreateTenantContextAsync(cancellationToken);
            }
            return _tenantContext;
        }

        protected virtual ITenantContext CreateTenantContext()
        {
            return new TenantContextBase();
        }

        protected virtual async Task<ITenantContext> CreateTenantContextAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new TenantContextBase());
        }
    }

    public abstract class TenantContextProviderOfT<TTenantContext>: ITenantContextProviderOfT<TTenantContext> 
        where TTenantContext: class, ITenantContext
    {
        protected readonly ILazyServiceProvider _serviceProvider;
        
        protected TTenantContext _tenantContext;
        
        private ILogger Logger => _serviceProvider.GetLogger(typeof(TenantContextProviderOfT<>));

        protected TenantContextProviderOfT(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITenantContext TenantContext => GetTenantContext();
        public TTenantContext TenantContextOfT => GetTenantContextOfT();
        
        public virtual ITenantContext GetTenantContext()
        {
            return GetTenantContextOfT();
        }

        public virtual async Task<ITenantContext> GetTenantContextAsync(CancellationToken cancellationToken = default)
        {
            return await GetTenantContextOfTAsync(cancellationToken);
        }

        public virtual TTenantContext GetTenantContextOfT()
        {
            if (_tenantContext == null)
            {
                _tenantContext = CreateTenantContext();
            }
            return _tenantContext;
        }

        public virtual async Task<TTenantContext> GetTenantContextOfTAsync(CancellationToken cancellationToken = default)
        {
            if (_tenantContext == null)
            {
                _tenantContext = await CreateTenantContextAsync(cancellationToken);
            }
            return _tenantContext;
        }

        protected abstract TTenantContext CreateTenantContext();

        protected abstract Task<TTenantContext> CreateTenantContextAsync(CancellationToken cancellationToken = default);
    }
}