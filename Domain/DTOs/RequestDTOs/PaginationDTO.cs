namespace ProductManagementSystem.Domain.DTOs.RequestDTOs
{
    public class PaginationDTO
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public string? SortBy { get; set; } = "Name";
        public string? SortOrder { get; set; } = "asc"; // asc or desc
    }
}
