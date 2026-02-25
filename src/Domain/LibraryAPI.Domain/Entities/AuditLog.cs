using LibraryAPI.Domain.Enums;
using System;

namespace LibraryAPI.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? BranchId { get; set; }
        public AuditType Type { get; set; }
        public string TableName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? AffectedColumns { get; set; }
        public string PrimaryKey { get; set; } = string.Empty;
    }
}
