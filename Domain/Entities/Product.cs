using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductManagementSystem.Domain.Entities
{
    [Index(nameof(SKU), IsUnique = true)]
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        public int StockQuantity { get; set; }
        
        [MaxLength(100)]
        public string? Category { get; set; }
        
        [MaxLength(50)]
        public string? SKU { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [Column(TypeName = "decimal(18,3)")]
        public decimal? Weight { get; set; }
        
        [MaxLength(500)]
        [Column(TypeName = "text")]
        public string? Tags { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        [Required]
        public string UserId { get; set; } = string.Empty; // Owner of the product
    }
}