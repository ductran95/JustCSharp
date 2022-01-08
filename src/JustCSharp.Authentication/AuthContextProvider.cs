using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.DependencyInjection;
using JustCSharp.Core.Logging.Extensions;
using Microsoft.Extensions.Logging;

namespace JustCSharp.Authentication
{
    public class AuthContextProvider: IAuthContextProvider
    {
        protected readonly ILazyServiceProvider _serviceProvider;

        protected IAuthContext _authContext;
        
        private ILogger Logger => _serviceProvider.GetLogger(typeof(AuthContextBase));

        public AuthContextProvider(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAuthContext AuthContext => GetAuthContext();
        
        public virtual IAuthContext GetAuthContext()
        {
            if (_authContext == null)
            {
                _authContext = CreateAuthContext();
            }
            return _authContext;
        }

        public virtual async Task<IAuthContext> GetAuthContextAsync(CancellationToken cancellationToken = default)
        {
            if (_authContext == null)
            {
                _authContext = await CreateAuthContextAsync(cancellationToken);
            }
            return _authContext;
        }

        protected virtual IAuthContext CreateAuthContext()
        {
            return new AuthContextBase();
        }

        protected virtual async Task<IAuthContext> CreateAuthContextAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new AuthContextBase());
        }
    }

    public abstract class AuthContextProvider<TAuthContext>: IAuthContextProvider<TAuthContext> 
        where TAuthContext: class, IAuthContext
    {
        protected readonly ILazyServiceProvider _serviceProvider;
        
        protected TAuthContext _authContext;
        
        private ILogger Logger => _serviceProvider.GetLogger(typeof(AuthContextProvider<>));

        protected AuthContextProvider(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        IAuthContext IAuthContextProvider.AuthContext => AuthContext;

        public TAuthContext AuthContext => GetAuthContext();

        IAuthContext IAuthContextProvider.GetAuthContext()
        {
            return GetAuthContext();
        }
        
        async Task<IAuthContext> IAuthContextProvider.GetAuthContextAsync(CancellationToken cancellationToken)
        {
            return await GetAuthContextAsync(cancellationToken);
        }

        public TAuthContext GetAuthContext()
        {
            if (_authContext == null)
            {
                _authContext = CreateAuthContext();
            }
            return _authContext;
        }
        
        public async Task<TAuthContext> GetAuthContextAsync(CancellationToken cancellationToken)
        {
            if (_authContext == null)
            {
                _authContext = await CreateAuthContextAsync(cancellationToken);
            }
            return _authContext;
        }

        protected abstract TAuthContext CreateAuthContext();

        protected abstract Task<TAuthContext> CreateAuthContextAsync(
            CancellationToken cancellationToken = default);
    }
}