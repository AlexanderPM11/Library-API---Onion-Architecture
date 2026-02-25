using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Application.DTOs;

namespace LibraryAPI.Application.Interfaces
{
    public interface IBranchService
    {
        Task<IEnumerable<Branch>> GetAllBranchesAsync();
        Task<Branch?> GetBranchByIdAsync(int id);
        Task<Branch> CreateBranchAsync(Branch branch);
        Task UpdateBranchAsync(Branch branch);
        Task DeleteBranchAsync(int id);
        Task<AuthResponseDto> SetupInitialBranchAsync(string userId, Branch branch);
    }
}
