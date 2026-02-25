using System;

namespace LibraryAPI.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        int? BranchId { get; }
        bool IsSuperAdmin { get; }
        bool IsAdmin { get; }
        bool IsEmpleado { get; }
    }
}
