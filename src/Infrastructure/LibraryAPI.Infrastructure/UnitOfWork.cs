using LibraryAPI.Domain.Interfaces;
using LibraryAPI.Infrastructure.Data;
using LibraryAPI.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace LibraryAPI.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext _context;
        public IBookRepository Books { get; private set; }
        public IAuthorRepository Authors { get; private set; }
        public ICategoryRepository Categories { get; private set; }
        public IBranchRepository Branches { get; private set; }
        public IAuditLogRepository AuditLogs { get; private set; }

        public UnitOfWork(LibraryDbContext context)
        {
            _context = context;
            Books = new BookRepository(_context);
            Authors = new AuthorRepository(_context);
            Categories = new CategoryRepository(_context);
            Branches = new BranchRepository(_context);
            AuditLogs = new AuditLogRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
