using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Services;
using ProductManagementSystem.Configs;
using ProductManagementSystem.Domain.DTOs.RequestDTOs;

namespace ProductManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;

        public OtpController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateOtp([FromBody] GenerateOtpRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseEntity(400, "Invalid request data", ModelState));

                await _otpService.GenerateOtpAsync(request.Email);
                return Ok(new ResponseEntity(200, "OTP sent successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseEntity(500, $"Failed to send OTP: {ex.Message}"));
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseEntity(400, "Invalid request data", ModelState));

                var isValid = await _otpService.ValidateOtpAsync(request.Email, request.Otp);
                if (!isValid)
                    return BadRequest(new ResponseEntity(400, "Invalid or expired OTP"));

                return Ok(new ResponseEntity(200, "OTP verified successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseEntity(500, $"Failed to verify OTP: {ex.Message}"));
            }
        }
    }
}