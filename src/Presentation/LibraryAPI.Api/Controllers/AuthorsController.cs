using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _authorService.GetAllAuthorsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _authorService.GetAuthorByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Empleado")]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDto authorDto)
        {
            var result = await _authorService.CreateAuthorAsync(authorDto);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Empleado")]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorCreateDto authorDto)
        {
            var result = await _authorService.UpdateAuthorAsync(id, authorDto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _authorService.DeleteAuthorAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
