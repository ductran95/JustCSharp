namespace JustCSharp.MultiTenancy
{
    public class TenantContextBase: ITenantContext
    {
        public string? TenantId { get; set; }
    }
}