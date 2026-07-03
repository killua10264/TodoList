using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs.Category;
using TodoListBackend.Services;

using Microsoft.AspNetCore.Authorization;

namespace TodoListBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService){
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            var responseList = categories.Select(c => new CategoryResponseDto {
                Id = c.Id,
                Name = c.Name,
                Color = c.Color
            });

            return Ok(responseList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            var newCategory = await _categoryService.CreateCategoryAsync(dto);
            if (newCategory == null)
            {
                return BadRequest(new { message = "Không thể tạo danh mục." });
            }
            return StatusCode(201, newCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, dto);
            if (updatedCategory == null)
            {
                return NotFound(new { message = $"Không tìm thấy danh mục có ID = {id}." });
            }
            return Ok(updatedCategory);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Không tìm thấy danh mục có ID = {id}." });
            }
            return NoContent();
        }
    }
}