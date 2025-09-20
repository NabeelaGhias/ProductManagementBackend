using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Configs;
using ProductManagementSystem.Domain.Models;

namespace ProductManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        [HttpGet("GetCategories")]
        public IActionResult GetCategories()
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
                new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items" },
                new Category { Id = 3, Name = "Home & Living", Description = "Home decor and furniture" },
                new Category { Id = 4, Name = "Books", Description = "Books and publications" },
                new Category { Id = 5, Name = "Sports", Description = "Sports equipment and gear" },
                new Category { Id = 6, Name = "Beauty", Description = "Beauty and personal care" },
                new Category { Id = 7, Name = "Toys", Description = "Toys and games" },
                new Category { Id = 8, Name = "Food & Beverages", Description = "Food and drink items" },
                new Category { Id = 9, Name = "Health", Description = "Health and wellness products" },
                new Category { Id = 10, Name = "Automotive", Description = "Car parts and accessories" }
            };

            return Ok(new ResponseEntity(200, "Categories retrieved successfully", categories));
        }
    }
}
