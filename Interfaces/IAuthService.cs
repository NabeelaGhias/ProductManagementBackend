using ProductManagementSystem.Domain.DTOs.RequestDTOs;
using ProductManagementSystem.Domain.DTOs.ResponseDTOs;

namespace ProductManagementSystem.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO request);
        Task<AuthResponseDTO?> RegisterAsync(RegisterRequestDTO request);
        Task<bool> UserExistsAsync(string email);
    }
}
