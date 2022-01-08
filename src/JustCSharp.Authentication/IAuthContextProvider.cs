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
    
    public interface IAuthContextProvider<TAuthContext>: IAuthContextProvider 
        where TAuthContext: class, IAuthContext
    {
        new TAuthContext AuthContext { get; }
        new TAuthContext GetAuthContext();
        new Task<TAuthContext> GetAuthContextAsync(CancellationToken cancellationToken = default);
    }
}