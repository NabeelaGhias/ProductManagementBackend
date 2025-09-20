using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.Domain.DTOs.RequestDTOs
{
    public class GenerateOtpRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
