using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Authentication
{
    public interface IAuthContextProvider
    {
        IAuthContext AuthContext { get; }
        IAuthContext GetAuthContext();
        Task<IAuthContext> GetAuthContextAsync(CancellationToken cancellationToken = default);
    }
    
    public interface IAuthContextProviderOfT<TAuthContext>: IAuthContextProvider 
        where TAuthContext: class, IAuthContext
    {
        TAuthContext AuthContextOfT { get; }
        TAuthContext GetAuthContextOfT();
        Task<TAuthContext> GetTAuthContextOfTAsync(CancellationToken cancellationToken = default);
    }
}