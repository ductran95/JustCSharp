using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Authentication
{
    public class AuthContextProvider: IAuthContextProvider
    {
        protected IAuthContext _authContext;

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

        public virtual IAuthContext CreateAuthContext()
        {
            return new AuthContextBase();
        }

        public virtual async Task<IAuthContext> CreateAuthContextAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new AuthContextBase());
        }
    }

    public abstract class AuthContextProviderOfT<TAuthContext>: IAuthContextProviderOfT<TAuthContext> 
        where TAuthContext: class, IAuthContext
    {
        protected TAuthContext _authContext;

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

        public abstract TAuthContext CreateAuthContext();

        public abstract Task<TAuthContext> CreateAuthContextAsync(CancellationToken cancellationToken = default);
    }
}