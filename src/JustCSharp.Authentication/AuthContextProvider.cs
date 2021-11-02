using System;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.DependencyInjection;
using JustCSharp.Core.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JustCSharp.Authentication
{
    public class AuthContextProvider: IAuthContextProvider
    {
        protected readonly ILazyServiceProvider _serviceProvider;

        protected IAuthContext _authContext;
        
        protected ILogger Logger => _serviceProvider.GetLogger(GetType());

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

    public abstract class AuthContextProviderOfT<TAuthContext>: IAuthContextProviderOfT<TAuthContext> 
        where TAuthContext: class, IAuthContext
    {
        protected readonly ILazyServiceProvider _serviceProvider;
        
        protected TAuthContext _authContext;
        
        protected ILogger Logger => _serviceProvider.GetLogger(GetType());

        protected AuthContextProviderOfT(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAuthContext AuthContext => GetAuthContext();
        public TAuthContext AuthContextOfT => GetAuthContextOfT();
        
        public virtual IAuthContext GetAuthContext()
        {
            return GetAuthContextOfT();
        }

        public virtual async Task<IAuthContext> GetAuthContextAsync(CancellationToken cancellationToken = default)
        {
            return await GetTAuthContextOfTAsync(cancellationToken);
        }

        public virtual TAuthContext GetAuthContextOfT()
        {
            if (_authContext == null)
            {
                _authContext = CreateAuthContext();
            }
            return _authContext;
        }

        public virtual async Task<TAuthContext> GetTAuthContextOfTAsync(CancellationToken cancellationToken = default)
        {
            if (_authContext == null)
            {
                _authContext = await CreateAuthContextAsync(cancellationToken);
            }
            return _authContext;
        }

        protected abstract TAuthContext CreateAuthContext();

        protected abstract Task<TAuthContext> CreateAuthContextAsync(CancellationToken cancellationToken = default);
    }
}