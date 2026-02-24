using LibraryAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Interfaces
{
    public interface IAuthorRepository : IGenericRepository<Author>
    {
        Task<IEnumerable<Author>> GetAuthorsWithDetailsAsync();
    }
}
