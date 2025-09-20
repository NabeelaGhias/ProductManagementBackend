using AutoMapper;
using ProductManagementSystem.Domain.DTOs.RequestDTOs;
using ProductManagementSystem.Domain.DTOs.ResponseDTOs;
using ProductManagementSystem.Domain.Entities;

namespace ProductManagementSystem.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Product mappings
            CreateMap<Product, ProductResponseDTO>();
            CreateMap<ProductRequestDTO, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
        }
    }
}
