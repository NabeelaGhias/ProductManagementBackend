namespace ProductManagementSystem.Domain.DTOs.ResponseDTOs
{
    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
