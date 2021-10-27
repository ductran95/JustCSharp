namespace JustCSharp.Authentication
{
    public class AuthContextAccessor: IAuthContextAccessor
    {
        public AuthContextBase AuthContext { get; set; }
    }
    
    public class AuthContextAccessor<TAuthContext> : IAuthContextAccessor<TAuthContext> where TAuthContext: AuthContextBase
    {
        public TAuthContext AuthContext { get; set; }
    }
}