using System;

namespace JustCSharp.Data.Entities
{
    public interface IAuditable
    {
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        
        void CheckAndSetAudit(string? currentUser);
    }
}