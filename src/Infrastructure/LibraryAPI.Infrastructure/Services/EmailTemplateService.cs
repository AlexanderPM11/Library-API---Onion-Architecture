using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Enums;
using LibraryAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly LibraryDbContext _context;

        public EmailTemplateService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<EmailTemplate?> GetActiveTemplateAsync(EmailTemplateType type, int? branchId)
        {
            // Cargar plantillas de este tipo
            var query = _context.EmailTemplates
                .AsNoTracking()
                .Where(t => t.Type == type && t.IsActive);

            // Intentar encontrar la de la id de sucursal específica
            if (branchId.HasValue)
            {
                var branchTemplate = await query.FirstOrDefaultAsync(t => t.BranchId == branchId.Value);
                if (branchTemplate != null)
                {
                    return branchTemplate;
                }
            }

            // Si no hay de sucursal específica (o es nula), retornar la Global (BranchId == null)
            return await query.FirstOrDefaultAsync(t => t.BranchId == null);
        }

        public async Task<IEnumerable<EmailTemplateDto>> GetAllTemplatesAsync(int? branchId, bool isSuperAdmin)
        {
            var query = _context.EmailTemplates.AsNoTracking();

            if (!isSuperAdmin)
            {
                // Un admin de sucursal solo ve las plantillas globales (null) o las de su propia sucursal.
                query = query.Where(t => t.BranchId == null || t.BranchId == branchId);
            }

            var templates = await query.ToListAsync();
            return templates.Select(t => new EmailTemplateDto
            {
                Id = t.Id,
                BranchId = t.BranchId,
                Subject = t.Subject,
                HtmlContent = t.HtmlContent,
                Type = t.Type,
                IsActive = t.IsActive
            });
        }

        public async Task<EmailTemplateDto?> GetTemplateByIdAsync(int id)
        {
            var t = await _context.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return null;

            return new EmailTemplateDto
            {
                Id = t.Id,
                BranchId = t.BranchId,
                Subject = t.Subject,
                HtmlContent = t.HtmlContent,
                Type = t.Type,
                IsActive = t.IsActive
            };
        }

        public async Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateDto dto, int? branchId, bool isSuperAdmin)
        {
            var entity = new EmailTemplate
            {
                BranchId = isSuperAdmin ? dto.BranchId : branchId, // Solo SuperAdmin puede crear globales o para otras sucursales
                Subject = dto.Subject,
                HtmlContent = dto.HtmlContent,
                Type = dto.Type,
                IsActive = dto.IsActive
            };

            _context.EmailTemplates.Add(entity);
            await _context.SaveChangesAsync();

            return new EmailTemplateDto
            {
                Id = entity.Id,
                BranchId = entity.BranchId,
                Subject = entity.Subject,
                HtmlContent = entity.HtmlContent,
                Type = entity.Type,
                IsActive = entity.IsActive
            };
        }

        public async Task<bool> UpdateTemplateAsync(int id, UpdateEmailTemplateDto dto, int? branchId, bool isSuperAdmin)
        {
            var entity = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null) return false;

            // Validar permisos
            if (!isSuperAdmin && entity.BranchId != branchId)
                return false; // No puede editar plantillas globales ni de otras sucursales

            entity.Subject = dto.Subject;
            entity.HtmlContent = dto.HtmlContent;
            entity.Type = dto.Type;
            entity.IsActive = dto.IsActive;

            _context.EmailTemplates.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTemplateAsync(int id, int? branchId, bool isSuperAdmin)
        {
            var entity = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null) return false;

            if (!isSuperAdmin && entity.BranchId != branchId)
                return false;

            _context.EmailTemplates.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
