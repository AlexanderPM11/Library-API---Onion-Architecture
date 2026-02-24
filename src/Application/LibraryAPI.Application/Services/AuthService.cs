using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto)
        {
            var user = new IdentityUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
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
    }
}
