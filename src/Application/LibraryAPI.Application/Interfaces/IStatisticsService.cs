using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;

namespace LibraryAPI.Application.Interfaces
{
    public interface IStatisticsService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<SuperAdminStatsDto> GetSuperAdminStatsAsync();
    }
}
