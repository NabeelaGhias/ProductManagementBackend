namespace ProductManagementSystem.Domain.DTOs.ResponseDTOs
{
    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Category { get; set; }
        public string? SKU { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Weight { get; set; }
        public string? Tags { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
