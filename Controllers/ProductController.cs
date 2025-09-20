using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Configs;
using ProductManagementSystem.Domain.DTOs.RequestDTOs;
using ProductManagementSystem.Interfaces;
using ProductManagementSystem.Helpers;

namespace ProductManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts([FromQuery] PaginationDTO pagination)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _productService.GetProductsAsync(pagination, userId);
                return Ok(new ResponseEntity(result));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("GetProduct/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var product = await _productService.GetProductByIdAsync(id, userId);
                
                if (product == null)
                    return NotFound(new ResponseEntity(404, "Product not found"));

                return Ok(new ResponseEntity(product));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseEntity(400, "Invalid request data", ModelState));

                var userId = GetCurrentUserId();
                var userName = GetCurrentUserName();

                // Handle base64 image if provided
                if (!string.IsNullOrEmpty(request.ImageBase64))
                {
                    try
                    {
                        var relativePath = await ImageHelper.SaveBase64ImageAsync(request.ImageBase64);
                        var baseUrl = $"{Request.Scheme}://{Request.Host}";
                        request.ImageUrl = ImageHelper.GetImageUrl(relativePath, baseUrl);
                    }
                    catch (ArgumentException ex)
                    {
                        return BadRequest(new ResponseEntity(400, $"Image processing error: {ex.Message}"));
                    }
                }

                var product = await _productService.CreateProductAsync(request, userId, userName);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new ResponseEntity(product));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequestDTO request)
        {
            try
            {
                if (id != request.Id)
                    return BadRequest(new ResponseEntity(400, "ID mismatch"));

                if (!ModelState.IsValid)
                    return BadRequest(new ResponseEntity(400, "Invalid request data", ModelState));

                var userId = GetCurrentUserId();
                var userName = GetCurrentUserName();

                // Handle image update - simple logic
                if (!string.IsNullOrEmpty(request.ImageBase64))
                {
                    try
                    {
                        // Get existing product to delete old image
                        var existingProduct = await _productService.GetProductByIdAsync(id, userId);
                        
                        // Delete old image if exists
                        if (existingProduct != null && !string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            var oldImagePath = existingProduct.ImageUrl.Replace($"{Request.Scheme}://{Request.Host}/", "");
                            ImageHelper.DeleteImage(oldImagePath);
                        }

                        // Save new image
                        var relativePath = await ImageHelper.SaveBase64ImageAsync(request.ImageBase64);
                        var baseUrl = $"{Request.Scheme}://{Request.Host}";
                        request.ImageUrl = ImageHelper.GetImageUrl(relativePath, baseUrl);
                    }
                    catch (ArgumentException ex)
                    {
                        return BadRequest(new ResponseEntity(400, $"Image processing error: {ex.Message}"));
                    }
                }
                else
                {
                    // No new image provided - preserve existing image URL
                    var existingProduct = await _productService.GetProductByIdAsync(id, userId);
                    if (existingProduct != null)
                    {
                        request.ImageUrl = existingProduct.ImageUrl;
                    }
                }

                var product = await _productService.UpdateProductAsync(request, userId, userName);

                return Ok(new ResponseEntity(product));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _productService.DeleteProductAsync(id, userId);

                if (!result)
                    return NotFound(new ResponseEntity(404, "Product not found"));

                return Ok(new ResponseEntity("Product deleted successfully"));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var userId = GetCurrentUserId();
                var categories = await _productService.GetCategoriesAsync(userId);
                return Ok(new ResponseEntity(categories));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("CheckExists/{id}")]
        public async Task<IActionResult> ProductExists(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var exists = await _productService.ProductExistsAsync(id, userId);
                return Ok(new ResponseEntity(exists));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
