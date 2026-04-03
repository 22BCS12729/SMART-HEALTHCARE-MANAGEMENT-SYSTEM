using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
    }
}
