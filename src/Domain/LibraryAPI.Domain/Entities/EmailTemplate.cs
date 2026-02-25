using System;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Domain.Entities
{
    public class EmailTemplate : BaseEntity
    {
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public EmailTemplateType Type { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Relación opcional con Branch. Si es NULL, es una plantilla "Global" (por defecto).
        public int? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }
    }
}
