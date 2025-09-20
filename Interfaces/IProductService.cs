using ProductManagementSystem.Domain.DTOs.RequestDTOs;
using ProductManagementSystem.Domain.DTOs.ResponseDTOs;

namespace ProductManagementSystem.Interfaces
{
    public interface IProductService
    {
        Task<PagedResultDTO<ProductResponseDTO>> GetProductsAsync(PaginationDTO pagination, string userId);
        Task<ProductResponseDTO?> GetProductByIdAsync(int id, string userId);
        Task<ProductResponseDTO> CreateProductAsync(ProductRequestDTO request, string userId, string userName);
        Task<ProductResponseDTO> UpdateProductAsync(ProductRequestDTO request, string userId, string userName);
        Task<bool> DeleteProductAsync(int id, string userId);
        Task<List<string>> GetCategoriesAsync(string userId);
        Task<bool> ProductExistsAsync(int id, string userId);
    }
}
