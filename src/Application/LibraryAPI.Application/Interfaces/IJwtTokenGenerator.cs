using System.Collections.Generic;
using System.Security.Claims;

namespace LibraryAPI.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string userId, string email, IEnumerable<string> roles);
    }
}
