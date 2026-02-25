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
        private readonly ICurrentUserService _currentUserService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
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
                    IsActive = user.IsActive,
                    BranchId = user.BranchId
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
                IsActive = user.IsActive,
                BranchId = user.BranchId
            };
        }

        public async Task<AuthResponseDto> CreateUserAsync(CreateUserDto createDto)
        { 
            try 
	        {
                var emailNormalized = createDto.Email.Trim().ToUpper();

                // Check if email or username already exists using a direct query for maximum reliability
                var emailExists = await _userManager.Users
                    .AnyAsync(u => u.NormalizedEmail == emailNormalized || u.NormalizedUserName == emailNormalized);

                if (emailExists)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "El correo electrónico o nombre de usuario ya está en uso."
                    };
                }

                var user = new ApplicationUser
                {
                    Email = createDto.Email.Trim(),
                    UserName = createDto.Email.Trim(),
                    FirstName = createDto.FirstName.Trim(),
                    LastName = createDto.LastName.Trim(),
                    IsActive = createDto.IsActive,
                    BranchId = _currentUserService.IsSuperAdmin ? createDto.BranchId : _currentUserService.BranchId
                };

                var result = await _userManager.CreateAsync(user, createDto.Password);

                if (!result.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Error de validación al crear el usuario.",
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
                    await _userManager.AddToRoleAsync(user, "Empleado"); // Fallback
                }

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Usuario creado exitosamente."
                };

            }
            catch (DbUpdateException)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "No se pudo guardar el usuario. El correo o nombre de usuario ya existe en el sistema."
                };
            }
	        catch (Exception ex)
	        {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error inesperado al procesar la solicitud.",
                    Errors = new List<string> { ex.Message }
                };
            }
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

            if (_currentUserService.IsSuperAdmin && updateDto.BranchId.HasValue)
            {
                user.BranchId = updateDto.BranchId.Value;
            }
            else if (!_currentUserService.IsSuperAdmin && updateDto.BranchId.HasValue && updateDto.BranchId != user.BranchId)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "No tiene permisos para cambiar la sucursal de este usuario." };
            }

            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return new AuthResponseDto { IsSuccess = false, Message = "Email already in use" };
                }

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
