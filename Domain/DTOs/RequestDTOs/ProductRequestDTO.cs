using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.Domain.DTOs.RequestDTOs
{
    public class ProductRequestDTO
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
        public int StockQuantity { get; set; }
        
        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string? Category { get; set; }
        
        [MaxLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string? SKU { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public string? ImageBase64 { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Weight must be non-negative")]
        public decimal? Weight { get; set; }
        
        [MaxLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
        public string? Tags { get; set; }
        
        public bool IsAvailable { get; set; } = true;
    }
}