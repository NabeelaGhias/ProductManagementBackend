using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using ProductManagementSystem.Domain.Settings;
using ProductManagementSystem.Domain.Models;

namespace ProductManagementSystem.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string email);
        Task<bool> ValidateOtpAsync(string email, string otp);
    }

    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly OtpSettings _otpSettings;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            IMemoryCache cache,
            IEmailService emailService,
            IOptions<OtpSettings> otpSettings,
            ILogger<OtpService> logger)
        {
            _cache = cache;
            _emailService = emailService;
            _otpSettings = otpSettings.Value;
            _logger = logger;
        }

        public async Task<string> GenerateOtpAsync(string email)
        {
            var otp = GenerateNumericOtp(_otpSettings.Length);
            var cacheKey = $"OTP_{email}";
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_otpSettings.ExpiryMinutes));

            _cache.Set(cacheKey, new OtpData
            {
                Code = otp,
                Attempts = 0,
                ExpiresAt = DateTime.Now.AddMinutes(_otpSettings.ExpiryMinutes)
            }, cacheEntryOptions);

            await SendOtpEmail(email, otp);
            return otp;
        }

        public Task<bool> ValidateOtpAsync(string email, string otp)
        {
            var cacheKey = $"OTP_{email}";
            if (!_cache.TryGetValue(cacheKey, out OtpData? otpData))
            {
                _logger.LogWarning($"OTP not found or expired for {email}");
                return Task.FromResult(false);
            }

            otpData!.Attempts++;
            if (otpData.Attempts > _otpSettings.AllowedAttempts)
            {
                _logger.LogWarning($"Maximum OTP attempts exceeded for {email}");
                _cache.Remove(cacheKey);
                return Task.FromResult(false);
            }

            if (DateTime.Now > otpData.ExpiresAt)
            {
                _logger.LogWarning($"OTP expired for {email}");
                _cache.Remove(cacheKey);
                return Task.FromResult(false);
            }

            if (otpData.Code != otp)
            {
                _logger.LogWarning($"Invalid OTP attempt for {email}");
                _cache.Set(cacheKey, otpData); // Update attempts count
                return Task.FromResult(false);
            }

            _cache.Remove(cacheKey);
            return Task.FromResult(true);
        }

        private string GenerateNumericOtp(int length)
        {
            var random = new Random();
            var otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10).ToString();
            }
            return otp;
        }

        private async Task SendOtpEmail(string email, string otp)
        {
            var subject = "Your Verification Code";
            var body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Email Verification</h2>
                        <p>Your verification code is: <strong style='font-size: 20px;'>{otp}</strong></p>
                        <p>This code will expire in {_otpSettings.ExpiryMinutes} minutes.</p>
                        <p>If you didn't request this code, please ignore this email.</p>
                    </body>
                </html>";

            await _emailService.SendEmailAsync(email, subject, body);
        }
    }
}