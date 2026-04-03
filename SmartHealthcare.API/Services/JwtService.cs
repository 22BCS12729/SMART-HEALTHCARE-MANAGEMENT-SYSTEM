using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Services
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }

    public class JwtService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IOptions<JwtSettings> jwtSettings, IUnitOfWork unitOfWork, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Email}", loginDto.Email);

                var user = await _unitOfWork.Repository<User>()
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login failed - User not found: {Email}", loginDto.Email);
                    return null;
                }

                if (!Helpers.PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed - Invalid password for: {Email}", loginDto.Email);
                    return null;
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                // Generate tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Login successful for user: {Email}", loginDto.Email);

                return new AuthResponseDto
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                    User = new UserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {Email}", loginDto.Email);
                return null;
            }
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>()
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && 
                                              u.RefreshTokenExpiryTime > DateTime.UtcNow);

                if (user == null)
                {
                    _logger.LogWarning("Token refresh failed - Invalid or expired refresh token");
                    return null;
                }

                // Generate new tokens
                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // Update refresh token
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

                return new AuthResponseDto
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                    User = new UserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>()
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

                if (user == null)
                    return false;

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Token revoked for user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                
                if (user == null)
                    return false;

                if (!Helpers.PasswordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                    return false;

                user.PasswordHash = Helpers.PasswordHasher.HashPassword(changePasswordDto.NewPassword);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password changed for user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user: {UserId}", userId);
                return false;
            }
        }

        private string GenerateAccessToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
