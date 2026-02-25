using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")] // Solo administradores pueden ver auditoría
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AuditLogDto>>>> GetAll()
        {
            var logs = await _auditLogService.GetAllAuditLogsAsync();
            return Ok(ApiResponse<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AuditLogDto>>> GetById(int id)
        {
            var log = await _auditLogService.GetAuditLogByIdAsync(id);
            if (log == null)
                return NotFound(ApiResponse<AuditLogDto>.FailureResponse("Audit log not found."));

            return Ok(ApiResponse<AuditLogDto>.SuccessResponse(log));
        }

        [HttpGet("table/{tableName}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AuditLogDto>>>> GetByTable(string tableName)
        {
            var logs = await _auditLogService.GetAuditLogsByTableAsync(tableName);
            return Ok(ApiResponse<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }
    }
}
