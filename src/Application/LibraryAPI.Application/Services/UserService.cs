using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive
                });
            }

            return userDtos;
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles.ToList(),
                IsActive = user.IsActive
            };
        }

        public async Task<AuthResponseDto> CreateUserAsync(CreateUserDto createDto)
        {
            var user = new ApplicationUser
            {
                Email = createDto.Email,
                UserName = createDto.Email,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                IsActive = createDto.IsActive
            };

            var result = await _userManager.CreateAsync(user, createDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Failed to create user",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Assign role. Check if role exists.
            var roleExists = await _roleManager.RoleExistsAsync(createDto.Role);
            if (roleExists)
            {
                await _userManager.AddToRoleAsync(user, createDto.Role);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "User"); // Fallback
            }

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "User created successfully"
            };
        }

        public async Task<AuthResponseDto> UpdateUserAsync(string id, UpdateUserDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "User not found" };
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            if (updateDto.IsActive.HasValue)
            {
                user.IsActive = updateDto.IsActive.Value;
            }

            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
            {
                user.Email = updateDto.Email;
                user.UserName = updateDto.Email;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Failed to update user details",
                    Errors = updateResult.Errors.Select(e => e.Description).ToList()
                };
            }

            // Update Password if provided
            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwdResult = await _userManager.ResetPasswordAsync(user, token, updateDto.Password);
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

            // Update Role if provided
            if (!string.IsNullOrEmpty(updateDto.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(updateDto.Role);
                if (roleExists)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(updateDto.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, updateDto.Role);
                    }
                }
            }

            return new AuthResponseDto { IsSuccess = true, Message = "User updated successfully" };
        }

    }
}
