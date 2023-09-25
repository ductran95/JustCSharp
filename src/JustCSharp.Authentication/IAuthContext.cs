using System.Net;

namespace JustCSharp.Authentication
{
    public interface IAuthContext
    {
        public string? UserId { get; set; }
        public string? Token { get; set; }
        public string? UserAgent { get; set; }
        public IPAddress? IP { get; set; }
    }
}