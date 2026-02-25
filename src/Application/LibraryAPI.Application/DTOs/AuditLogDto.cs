using System;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Application.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public AuditType Type { get; set; }
        public string TypeName => Type.ToString();
        public string TableName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? AffectedColumns { get; set; }
        public string PrimaryKey { get; set; } = string.Empty;
    }
}
