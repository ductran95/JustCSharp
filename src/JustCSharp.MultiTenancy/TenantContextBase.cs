namespace JustCSharp.Authentication
{
    public class TenantContextBase: ITenantContext
    {
        public string TenantId { get; set; }
    }
}