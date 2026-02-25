using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchesController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Branch>>> GetAll()
        {
            var branches = await _branchService.GetAllBranchesAsync();
            return Ok(branches);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Branch>> GetById(int id)
        {
            var branch = await _branchService.GetBranchByIdAsync(id);
            if (branch == null) return NotFound();
            return Ok(branch);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<Branch>> Create(Branch branch)
        {
            var createdBranch = await _branchService.CreateBranchAsync(branch);
            return CreatedAtAction(nameof(GetById), new { id = createdBranch.Id }, createdBranch);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Branch branch)
        {
            if (id != branch.Id) return BadRequest();
            await _branchService.UpdateBranchAsync(branch);
            return NoContent();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _branchService.DeleteBranchAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("setup")]
        public async Task<ActionResult<AuthResponseDto>> Setup(Branch branch)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await _branchService.SetupInitialBranchAsync(userId, branch);
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }
    }
}
