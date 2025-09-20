using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ProductManagementSystem.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        public string FullName => $"{FirstName} {LastName}";
    }
}