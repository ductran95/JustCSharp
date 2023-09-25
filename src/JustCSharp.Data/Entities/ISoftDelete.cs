using System;

namespace JustCSharp.Data.Entities
{
    public interface ISoftDelete
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        
        void CheckAndSetDeleteAudit(string? currentUser);
        bool IsHardDeleted();
    }
}