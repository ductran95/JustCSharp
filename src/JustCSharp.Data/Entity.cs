using System;

namespace JustCSharp.Data
{
    public class Entity: IEntity, ISoftDelete, IAuditable
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        
        public void CheckAndSetId()
        {
            if (Id == Guid.Empty)
            {
                return;
            }
            
            Id = Guid.NewGuid();
        }
        
        public void CheckAndSetAudit(string currentUser)
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
        
        public void CheckAndSetDeleteAudit(string currentUser)
        {
            var curTime = DateTime.UtcNow;
            DeletedOn = curTime;
            DeletedBy = currentUser;
        }

        public bool IsHardDeleted()
        {
            return IsDeleted;
        }
    }
}