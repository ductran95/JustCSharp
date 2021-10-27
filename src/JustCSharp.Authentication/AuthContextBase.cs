using System.Net;

namespace JustCSharp.Authentication
{
    public abstract class AuthContextBase
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string UserAgent { get; set; }
        public IPAddress IP { get; set; }
    }
}