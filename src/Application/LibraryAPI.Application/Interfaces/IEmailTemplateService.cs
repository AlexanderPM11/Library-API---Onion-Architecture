using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Application.Interfaces
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplate?> GetActiveTemplateAsync(EmailTemplateType type, int? branchId);
        Task<IEnumerable<EmailTemplateDto>> GetAllTemplatesAsync(int? branchId, bool isSuperAdmin);
        Task<EmailTemplateDto?> GetTemplateByIdAsync(int id);
        Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateDto dto, int? branchId, bool isSuperAdmin);
        Task<bool> UpdateTemplateAsync(int id, UpdateEmailTemplateDto dto, int? branchId, bool isSuperAdmin);
        Task<bool> DeleteTemplateAsync(int id, int? branchId, bool isSuperAdmin);
    }
}
