using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto?> GetByEmailAsync(string email);
        Task<PagedResultDto<UserDto>> GetAllAsync(PaginationParams paginationParams);
        Task<UserDto> CreateAsync(CreateUserDto createUserDto);
        Task<UserDto?> UpdateAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string email);
    }
}
