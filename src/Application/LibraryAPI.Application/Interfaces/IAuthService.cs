using LibraryAPI.Application.DTOs;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(UserLoginDto loginDto);
    }
}
