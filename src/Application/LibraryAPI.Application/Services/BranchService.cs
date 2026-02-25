using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces;
using LibraryAPI.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace LibraryAPI.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public BranchService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<IEnumerable<Branch>> GetAllBranchesAsync()
        {
            return await _unitOfWork.Branches.GetAllAsync();
        }

        public async Task<Branch?> GetBranchByIdAsync(int id)
        {
            return await _unitOfWork.Branches.GetByIdAsync(id);
        }

        public async Task<Branch> CreateBranchAsync(Branch branch)
        {
            await _unitOfWork.Branches.AddAsync(branch);
            await _unitOfWork.CompleteAsync();
            return branch;
        }

        public async Task UpdateBranchAsync(Branch branch)
        {
            _unitOfWork.Branches.Update(branch);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteBranchAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch != null)
            {
                _unitOfWork.Branches.Remove(branch);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<AuthResponseDto> SetupInitialBranchAsync(string userId, Branch branch)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Usuario no encontrado" };
            }

            if (user.BranchId != null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "El usuario ya tiene una sucursal asignada" };
            }

            // Create the branch
            await _unitOfWork.Branches.AddAsync(branch);
            await _unitOfWork.CompleteAsync();

            // Link user to branch
            user.BranchId = branch.Id;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new AuthResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "Error al vincular el usuario con la sucursal",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Generate new token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email!, roles, user.BranchId);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Sucursal configurada con éxito",
                Token = token
            };
        }
    }
}
