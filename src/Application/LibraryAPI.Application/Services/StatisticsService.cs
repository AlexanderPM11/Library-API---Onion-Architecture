using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces;

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
            var books = (await _unitOfWork.Books.GetAllAsync()).ToList();
            var authors = (await _unitOfWork.Authors.GetAllAsync()).ToList();
            var categories = (await _unitOfWork.Categories.GetAllAsync()).ToList();

            var recentActivities = new List<RecentActivityDto>();

            // Recent Books
            recentActivities.AddRange(books
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .Select(b => new RecentActivityDto
                {
                    Title = "Nuevo Libro",
                    Message = $"Se añadió el libro \"{b.Title}\"",
                    Type = "Book",
                    Date = b.CreatedAt.ToString("g")
                }));

            // Recent Authors
            recentActivities.AddRange(authors
                .OrderByDescending(a => a.CreatedAt)
                .Take(2)
                .Select(a => new RecentActivityDto
                {
                    Title = "Nuevo Autor",
                    Message = $"Se registró a {a.FirstName} {a.LastName}",
                    Type = "Author",
                    Date = a.CreatedAt.ToString("g")
                }));

            // Recent Categories
            recentActivities.AddRange(categories
                .OrderByDescending(c => c.CreatedAt)
                .Take(2)
                .Select(c => new RecentActivityDto
                {
                    Title = "Nueva Categoría",
                    Message = $"Se creó la categoría \"{c.Name}\"",
                    Type = "Category",
                    Date = c.CreatedAt.ToString("g")
                }));

            return new DashboardStatsDto
            {
                TotalBooks = books.Count,
                TotalAuthors = authors.Count,
                TotalCategories = categories.Count,
                RecentActivities = recentActivities.OrderByDescending(x => x.Date).Take(10).ToList()
            };
        }
    }
}
