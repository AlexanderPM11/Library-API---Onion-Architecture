using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace LibraryAPI.Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
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

            // Books by Category for Chart
            var booksByCategory = books
                .GroupBy(b => b.CategoryId)
                .Select(g => new CategoryDistributionDto
                {
                    CategoryName = categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Sin Categoría",
                    BookCount = g.Count()
                })
                .OrderByDescending(x => x.BookCount)
                .ToList();

            return new DashboardStatsDto
            {
                TotalBooks = books.Count,
                TotalAuthors = authors.Count,
                TotalCategories = categories.Count,
                RecentActivities = recentActivities.OrderByDescending(x => x.Date).Take(10).ToList(),
                BooksByCategory = booksByCategory
            };
        }

        public async Task<SuperAdminStatsDto> GetSuperAdminStatsAsync()
        {
            var branches = (await _unitOfWork.Branches.GetAllIgnoreFiltersAsync()).ToList();
            var users = await _userManager.Users.IgnoreQueryFilters().ToListAsync();
            var books = (await _unitOfWork.Books.GetAllIgnoreFiltersAsync()).ToList();

            var topBranches = books
                .GroupBy(b => b.BranchId)
                .Select(g => new BranchBookCountDto
                {
                    BranchName = branches.FirstOrDefault(b => b.Id == g.Key)?.Name ?? "Sin Sucursal",
                    BookCount = g.Count()
                })
                .OrderByDescending(x => x.BookCount)
                .Take(5)
                .ToList();

            var globalActivities = new List<RecentActivityDto>();

            // Recent Branches
            globalActivities.AddRange(branches
                .OrderByDescending(b => b.Id) // Assuming ID is incremental
                .Take(3)
                .Select(b => new RecentActivityDto
                {
                    Title = "Nueva Sucursal",
                    Message = $"Se registró la sucursal \"{b.Name}\"",
                    Type = "Branch",
                    Date = DateTime.Now.ToString("g") // Branches don't have CreatedAt in DTO, ideally would have it
                }));

            // Recent Users
            globalActivities.AddRange(users
                .Take(3)
                .Select(u => new RecentActivityDto
                {
                    Title = "Nuevo Usuario",
                    Message = $"Se registró el usuario {u.Email}",
                    Type = "User",
                    Date = DateTime.Now.ToString("g")
                }));

            return new SuperAdminStatsDto
            {
                TotalBranches = branches.Count,
                TotalActiveUsers = users.Count(u => u.IsActive),
                TotalSystemBooks = books.Count,
                ActiveBranchesCount = branches.Count(b => b.IsActive),
                TopBranchesByBooks = topBranches,
                GlobalActivities = globalActivities.OrderByDescending(x => x.Date).ToList()
            };
        }
    }
}
