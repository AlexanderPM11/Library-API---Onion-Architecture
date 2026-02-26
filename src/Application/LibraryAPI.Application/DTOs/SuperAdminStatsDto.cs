using System.Collections.Generic;

namespace LibraryAPI.Application.DTOs
{
    public class SuperAdminStatsDto
    {
        public int TotalBranches { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalSystemBooks { get; set; }
        public int ActiveBranchesCount { get; set; }
        public List<BranchBookCountDto> TopBranchesByBooks { get; set; } = new();
        public List<RecentActivityDto> GlobalActivities { get; set; } = new();
    }

    public class BranchBookCountDto
    {
        public string? BranchName { get; set; }
        public int BookCount { get; set; }
    }
}
