using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs.Category;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/categories")]
    public class CategoryController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            int userId = GetCurrentUserId();
            var categories = await _categoryService.GetAllCategoriesAsync(userId);

            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            int userId = GetCurrentUserId();

            var newCategory = await _categoryService.CreateCategoryAsync(dto, userId);
            if (newCategory == null)
            {
                return BadRequest(new { message = "Không thể tạo danh mục." });
            }

            return StatusCode(201, newCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, dto, userId);
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
            int userId = GetCurrentUserId();

            var result = await _categoryService.DeleteCategoryAsync(id, userId);
            if (!result)
            {
                return NotFound(new { message = $"Không tìm thấy danh mục có ID = {id}." });
            }

            return NoContent();
        }
    }
}