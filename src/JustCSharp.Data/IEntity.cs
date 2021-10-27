using System;

namespace JustCSharp.Data
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