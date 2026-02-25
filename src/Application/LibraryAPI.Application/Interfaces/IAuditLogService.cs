using LibraryAPI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogDto>> GetAllAuditLogsAsync();
        Task<IEnumerable<AuditLogDto>> GetAuditLogsByTableAsync(string tableName);
        Task<AuditLogDto?> GetAuditLogByIdAsync(int id);
    }
}
