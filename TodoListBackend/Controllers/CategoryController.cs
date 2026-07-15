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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            int userId = GetCurrentUserId();
            var category = await _categoryService.GetCategoryByIdAsync(id, userId);
            if (category == null) return NotFound(new { message = "Không tìm thấy danh mục." });

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            int userId = GetCurrentUserId();

            var newCategory = await _categoryService.CreateCategoryAsync(dto, userId);

            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, newCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, dto, userId);

            return Ok(updatedCategory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            int userId = GetCurrentUserId();

            await _categoryService.DeleteCategoryAsync(id, userId);

            return NoContent();
        }
    }
}
