using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatisticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var booksCount = await _unitOfWork.Repository<Domain.Entities.Book>().Entities.CountAsync();
            var authorsCount = await _unitOfWork.Repository<Domain.Entities.Author>().Entities.CountAsync();
            var categoriesCount = await _unitOfWork.Repository<Domain.Entities.Category>().Entities.CountAsync();

            var recentBooks = await _unitOfWork.Repository<Domain.Entities.Book>().Entities
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new RecentActivityDto
                {
                    Title = "Nuevo Libro",
                    Message = $"Se añadió el libro \"{b.Title}\"",
                    Type = "Book",
                    Date = b.CreatedAt.ToString("g")
                })
                .ToListAsync();

            return new DashboardStatsDto
            {
                TotalBooks = booksCount,
                TotalAuthors = authorsCount,
                TotalCategories = categoriesCount,
                RecentActivities = recentBooks
            };
        }
    }
}
