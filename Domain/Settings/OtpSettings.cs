namespace ProductManagementSystem.Domain.Settings
{
    public class OtpSettings
    {
        public int ExpiryMinutes { get; set; }
        public int Length { get; set; }
        public int AllowedAttempts { get; set; }
    }
}
