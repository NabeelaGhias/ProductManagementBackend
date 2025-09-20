using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Configs;
using ProductManagementSystem.Domain.DTOs.RequestDTOs;
using ProductManagementSystem.Interfaces;

namespace ProductManagementSystem.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseEntity(400, "Invalid request data", ModelState));

                var result = await _authService.LoginAsync(request);
                if (result == null)
                    return Unauthorized(new ResponseEntity(401, "Invalid email or password"));

                return Ok(new ResponseEntity(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseEntity(500, $"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseEntity(400, "Invalid request data", ModelState));

                var userExists = await _authService.UserExistsAsync(request.Email);
                if (userExists)
                    return BadRequest(new ResponseEntity(400, "User with this email already exists"));

                var result = await _authService.RegisterAsync(request);
                if (result == null)
                    return BadRequest(new ResponseEntity(400, "Failed to create user"));

                return CreatedAtAction(nameof(Login), new ResponseEntity(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseEntity(500, $"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            try
            {
                var exists = await _authService.UserExistsAsync(email);
                return Ok(new ResponseEntity(new { exists }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseEntity(500, $"An error occurred: {ex.Message}"));
            }
        }
    }
}
