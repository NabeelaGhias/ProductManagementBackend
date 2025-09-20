namespace ProductManagementSystem.Domain.Models
{
    public class OtpData
    {
        public string Code { get; set; } = string.Empty;
        public int Attempts { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
