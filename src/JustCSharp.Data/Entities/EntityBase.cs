using System;

namespace JustCSharp.Data.Entities
{
    public abstract class EntityBase<TKey>: IEntity<TKey>, ISoftDelete, IAuditable
    {
        public TKey Id { get; set; } = default!;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }

        public virtual object GetKey()
        {
            return Id;
        }

        public abstract void CheckAndSetId();
        
        public virtual void CheckAndSetAudit(string currentUser)
        {
            var curTime = DateTime.UtcNow;

            ModifiedOn = curTime;
            ModifiedBy = currentUser;

            if (CreatedOn == null)
            {
                CreatedOn = curTime;
                CreatedBy = currentUser;
            }
        }
        
        public virtual void CheckAndSetDeleteAudit(string currentUser)
        {
            var curTime = DateTime.UtcNow;
            DeletedOn = curTime;
            DeletedBy = currentUser;
        }

        public virtual bool IsHardDeleted()
        {
            return IsDeleted;
        }
    }
}