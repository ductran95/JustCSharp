namespace JustCSharp.MultiTenancy
{
    public interface ITenantContext
    {
        public string TenantId { get; set; }
    }
}