using System;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IBookRepository Books { get; }
        IAuthorRepository Authors { get; }
        ICategoryRepository Categories { get; }
        IAuditLogRepository AuditLogs { get; }
        Task<int> CompleteAsync();
    }
}
