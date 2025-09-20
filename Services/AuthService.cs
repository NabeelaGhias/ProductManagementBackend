using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProductManagementSystem.Domain.DTOs.RequestDTOs;
using ProductManagementSystem.Domain.DTOs.ResponseDTOs;
using ProductManagementSystem.Domain.Entities;
using ProductManagementSystem.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProductManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return null;

            if (!user.IsActive)
                throw new InvalidOperationException("Account is deactivated. Please contact support.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.IsLockedOutAsync(user))
                    throw new InvalidOperationException("Account is locked. Please try again later.");
                    
                return null;
            }

            // Reset lockout on successful login
            await _userManager.ResetAccessFailedCountAsync(user);
            
            // Update last login
            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);

            var token = await GenerateJwtTokenAsync(user);
            var expiresAt = DateTime.Now.AddHours(24);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDTO?> RegisterAsync(RegisterRequestDTO request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            var token = await GenerateJwtTokenAsync(user);
            var expiresAt = DateTime.Now.AddHours(24);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                ExpiresAt = expiresAt
            };
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        private async Task<string> GenerateJwtTokenAsync(AppUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add user roles to claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}