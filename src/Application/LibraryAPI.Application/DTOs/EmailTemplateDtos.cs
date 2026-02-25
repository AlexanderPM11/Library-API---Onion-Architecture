using System;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Application.DTOs
{
    public class EmailTemplateDto
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public EmailTemplateType Type { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEmailTemplateDto
    {
        public int? BranchId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public EmailTemplateType Type { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmailTemplateDto
    {
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public EmailTemplateType Type { get; set; }
        public bool IsActive { get; set; }
    }
}
