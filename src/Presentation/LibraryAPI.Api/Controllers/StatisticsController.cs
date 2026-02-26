using LibraryAPI.Application.Interfaces;
using LibraryAPI.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LibraryAPI.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _statisticsService.GetDashboardStatsAsync();
            return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("super-admin")]
        public async Task<IActionResult> GetSuperAdminStats()
        {
            var stats = await _statisticsService.GetSuperAdminStatsAsync();
            return Ok(ApiResponse<SuperAdminStatsDto>.SuccessResponse(stats));
        }
    }
}
