using System;
namespace Sakolee.Domain.Entities
{
    public class FamilyStatus
    {
        public Guid FamilyStatusId { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}