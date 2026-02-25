using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

using LibraryAPI.Domain.Entities;

namespace LibraryAPI.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User registration failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "User registered successfully"
            };
        }

        public async Task<AuthResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid credentials"
                };
            }

            // Get roles
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email!, roles);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token
            };
        }
        public async Task<UserProfileDto?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "User not found" };
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Failed to update profile details",
                    Errors = updateResult.Errors.Select(e => e.Description).ToList()
                };
            }

            if (!string.IsNullOrEmpty(updateDto.CurrentPassword) && !string.IsNullOrEmpty(updateDto.NewPassword))
            {
                var pwdResult = await _userManager.ChangePasswordAsync(user, updateDto.CurrentPassword, updateDto.NewPassword);
                if (!pwdResult.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Failed to update password",
                        Errors = pwdResult.Errors.Select(e => e.Description).ToList()
                    };
                }
            }

            return new AuthResponseDto { IsSuccess = true, Message = "Profile updated successfully" };
        }
    }
}
