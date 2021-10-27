namespace JustCSharp.Data
{
    public interface IHasConcurrencyStamp
    {
        string ConcurrencyStamp { get; set; }
        
        string CheckAndSetConcurrencyStamp();
    }
}