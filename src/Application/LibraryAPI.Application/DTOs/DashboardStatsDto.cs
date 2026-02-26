using System.Collections.Generic;

namespace LibraryAPI.Application.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalBooks { get; set; }
        public int TotalAuthors { get; set; }
        public int TotalCategories { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<CategoryDistributionDto> BooksByCategory { get; set; } = new();
    }

    public class CategoryDistributionDto
    {
        public string? CategoryName { get; set; }
        public int BookCount { get; set; }
    }

    public class RecentActivityDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // e.g., "Book", "Author", "Category"
        public string Date { get; set; }
    }
}
