using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class EmailTemplatesController : ControllerBase
    {
        private readonly IEmailTemplateService _templateService;
        private readonly ITemplateEngineService _engineService;
        private readonly ICurrentUserService _currentUserService;

        public EmailTemplatesController(
            IEmailTemplateService templateService,
            ITemplateEngineService engineService,
            ICurrentUserService currentUserService)
        {
            _templateService = templateService;
            _engineService = engineService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetAll()
        {
            var templates = await _templateService.GetAllTemplatesAsync(_currentUserService.BranchId, _currentUserService.IsSuperAdmin);
            return Ok(ApiResponse<IEnumerable<EmailTemplateDto>>.SuccessResponse(templates));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmailTemplateDto>> GetById(int id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null) return NotFound(ApiResponse<object>.FailureResponse("Plantilla no encontrada."));

            // Permisos (Un Admin intentando leer una plantilla de otra sucursal)
            if (!_currentUserService.IsSuperAdmin && template.BranchId != null && template.BranchId != _currentUserService.BranchId)
                return Forbid();

            return Ok(ApiResponse<EmailTemplateDto>.SuccessResponse(template));
        }

        [HttpPost]
        public async Task<ActionResult<EmailTemplateDto>> Create(CreateEmailTemplateDto dto)
        {
            var result = await _templateService.CreateTemplateAsync(dto, _currentUserService.BranchId, _currentUserService.IsSuperAdmin);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<EmailTemplateDto>.SuccessResponse(result, "Plantilla creada correctamente."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateEmailTemplateDto dto)
        {
            var success = await _templateService.UpdateTemplateAsync(id, dto, _currentUserService.BranchId, _currentUserService.IsSuperAdmin);
            if (!success) return BadRequest(ApiResponse<object>.FailureResponse("No se pudo actualizar la plantilla o no tiene permisos."));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Plantilla actualizada correctamente."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _templateService.DeleteTemplateAsync(id, _currentUserService.BranchId, _currentUserService.IsSuperAdmin);
            if (!success) return BadRequest(ApiResponse<object>.FailureResponse("No se pudo eliminar la plantilla o no tiene permisos."));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Plantilla eliminada exitosamente."));
        }

        [HttpPost("preview")]
        public ActionResult<string> PreviewTemplate([FromBody] PreviewRequestDto request)
        {
            var variables = request.Variables ?? new Dictionary<string, string>();
            var html = _engineService.RenderTemplate(request.HtmlContent, variables);
            
            // Retorna texto plano HTML en formato JSON 
            return Ok(new { html });
        }
    }

    public class PreviewRequestDto
    {
        public string HtmlContent { get; set; } = string.Empty;
        public Dictionary<string, string>? Variables { get; set; }
    }
}
