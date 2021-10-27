using System;

namespace JustCSharp.Data
{
    public interface IEntity
    {
        void CheckAndSetId();
    }
    
    public interface IEntity<TKey>: IEntity
    {
        public TKey Id { get; set; }
    }
}