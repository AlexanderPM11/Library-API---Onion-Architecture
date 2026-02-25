using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;

namespace LibraryAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<AuthResponseDto> CreateUserAsync(CreateUserDto createDto);
        Task<AuthResponseDto> UpdateUserAsync(string id, UpdateUserDto updateDto);
    }
}
