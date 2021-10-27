namespace JustCSharp.Authentication
{
    public interface IAuthContextAccessor
    {
        AuthContextBase AuthContext { get; set; }
    }
    
    public interface IAuthContextAccessor<TAuthContext> where TAuthContext: AuthContextBase
    {
        TAuthContext AuthContext { get; set; }
    }
}