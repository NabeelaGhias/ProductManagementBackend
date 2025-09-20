using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.Domain.DTOs.RequestDTOs;
using ProductManagementSystem.Domain.DTOs.ResponseDTOs;
using ProductManagementSystem.Domain.Entities;
using ProductManagementSystem.Domain.Repository;
using ProductManagementSystem.Interfaces;
using ProductManagementSystem.Helpers;

namespace ProductManagementSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IRepository<Product> productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDTO<ProductResponseDTO>> GetProductsAsync(PaginationDTO pagination, string userId)
        {
            var query = _productRepository.TableNoTracking
                .Where(p => p.UserId == userId && !p.IsDeleted);

            // Search filter
            if (!string.IsNullOrEmpty(pagination.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(pagination.SearchTerm) ||
                                   p.Description!.Contains(pagination.SearchTerm) ||
                                   p.SKU!.Contains(pagination.SearchTerm));
            }

            // Category filter
            if (!string.IsNullOrEmpty(pagination.Category))
            {
                query = query.Where(p => p.Category == pagination.Category);
            }

            // Sorting
            query = pagination.SortBy?.ToLower() switch
            {
                "name" => pagination.SortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "price" => pagination.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "stock" => pagination.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.StockQuantity)
                    : query.OrderBy(p => p.StockQuantity),
                "created" => pagination.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.CreatedDate)
                    : query.OrderBy(p => p.CreatedDate),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var productDTOs = _mapper.Map<List<ProductResponseDTO>>(items);

            return new PagedResultDTO<ProductResponseDTO>
            {
                Items = productDTOs,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        public async Task<ProductResponseDTO?> GetProductByIdAsync(int id, string userId)
        {
            var product = await _productRepository.FirstOrDefaultAsync(p => 
                p.Id == id && p.UserId == userId && !p.IsDeleted);
            
            return product == null ? null : _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<ProductResponseDTO> CreateProductAsync(ProductRequestDTO request, string userId, string userName)
        {
            var product = _mapper.Map<Product>(request);
            product.UserId = userId;
            product.CreatedBy = userName;
            product.CreatedDate = DateTime.UtcNow;

            var createdProduct = await _productRepository.AddAsync(product);
            return _mapper.Map<ProductResponseDTO>(createdProduct);
        }

        public async Task<ProductResponseDTO> UpdateProductAsync(ProductRequestDTO request, string userId, string userName)
        {
            var existingProduct = await _productRepository.FirstOrDefaultAsync(p => 
                p.Id == request.Id && p.UserId == userId && !p.IsDeleted);

            if (existingProduct == null)
                throw new ApiException(404, "Product not found");

            _mapper.Map(request, existingProduct);
            existingProduct.ModifiedBy = userName;
            existingProduct.ModifiedDate = DateTime.UtcNow;

            await _productRepository.UpdateAsync(existingProduct);
            return _mapper.Map<ProductResponseDTO>(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(int id, string userId)
        {
            var product = await _productRepository.FirstOrDefaultAsync(p => 
                p.Id == id && p.UserId == userId && !p.IsDeleted);

            if (product == null)
                return false;

            product.IsDeleted = true;
            product.ModifiedDate = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);
            
            return true;
        }

        public async Task<List<string>> GetCategoriesAsync(string userId)
        {
            return await _productRepository.TableNoTracking
                .Where(p => p.UserId == userId && !p.IsDeleted && !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<bool> ProductExistsAsync(int id, string userId)
        {
            return await _productRepository.AnyAsync(p => 
                p.Id == id && p.UserId == userId && !p.IsDeleted);
        }
    }
}
