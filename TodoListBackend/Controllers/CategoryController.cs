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

            // FIX 4.2: Xóa null-check dead code — Service sẽ throw exception nếu lỗi
            var newCategory = await _categoryService.CreateCategoryAsync(dto, userId);

            return StatusCode(201, newCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            // FIX 4.2: Xóa null-check dead code
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, dto, userId);

            return Ok(updatedCategory);
        }

        // FIX 3.6: Xóa [Authorize(Roles = "Admin")] — cho phép user thường xóa category CỦA CHÍNH MÌNH (đã filter theo userId trong service)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            int userId = GetCurrentUserId();

            // FIX 4.2: Xóa false check dead code
            await _categoryService.DeleteCategoryAsync(id, userId);

            return NoContent();
        }
    }
}