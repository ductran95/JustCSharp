namespace JustCSharp.Data.Entities
{
    public interface IEntity
    {
        object GetKey();
        void CheckAndSetId();
    }
    
    public interface IEntity<TKey>: IEntity
    {
        public TKey Id { get; set; }
    }
}