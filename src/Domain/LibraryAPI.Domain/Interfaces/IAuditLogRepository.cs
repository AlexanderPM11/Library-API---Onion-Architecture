using LibraryAPI.Domain.Entities;

namespace LibraryAPI.Domain.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
    }
}
