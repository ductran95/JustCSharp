using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Authentication
{
    public interface IAuthContextProvider
    {
        AuthContextBase GetAuthContextBase();
        Task<AuthContextBase> GetAuthContextBaseAsync(CancellationToken cancellationToken = default);
    }
    
    public interface IAuthContextProvider<TAuthContext>: IAuthContextProvider where TAuthContext: AuthContextBase
    {
        TAuthContext GetAuthContext();
        Task<TAuthContext> GetAuthContextAsync(CancellationToken cancellationToken = default);
    }
}