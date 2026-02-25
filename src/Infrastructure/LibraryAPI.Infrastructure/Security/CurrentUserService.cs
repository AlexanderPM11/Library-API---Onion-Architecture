using System.Security.Claims;
using LibraryAPI.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LibraryAPI.Infrastructure.Security
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public int? BranchId
        {
            get
            {
                var branchIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("BranchId");
                return int.TryParse(branchIdClaim, out var branchId) ? branchId : null;
            }
        }

        public bool IsSuperAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("SuperAdmin") ?? false;
        public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
        public bool IsEmpleado => _httpContextAccessor.HttpContext?.User?.IsInRole("Empleado") ?? false;
    }
}
