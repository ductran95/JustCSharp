namespace JustCSharp.Data.Entities
{
    public interface IHasConcurrencyStamp
    {
        string ConcurrencyStamp { get; set; }
        
        string CheckAndSetConcurrencyStamp();
    }
}