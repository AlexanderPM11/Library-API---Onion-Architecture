using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces;
using LibraryAPI.Infrastructure.Data;

namespace LibraryAPI.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(LibraryDbContext context) : base(context)
        {
        }
    }
}
