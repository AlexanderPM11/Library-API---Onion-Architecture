using LibraryAPI.Application.DTOs;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(UserLoginDto loginDto);
        Task<UserProfileDto?> GetProfileAsync(string userId);
        Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto updateDto);
        Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto confirmDto);
        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
