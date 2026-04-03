using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.API.Helpers;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<PagedResultDto<UserDto>> GetAllAsync(PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<User>().Query()
                .Where(u => u.IsActive);

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = paginationParams.SortBy?.ToLower() switch
            {
                "email" => paginationParams.IsDescending 
                    ? query.OrderByDescending(u => u.Email) 
                    : query.OrderBy(u => u.Email),
                "lastname" => paginationParams.IsDescending 
                    ? query.OrderByDescending(u => u.LastName) 
                    : query.OrderBy(u => u.LastName),
                "role" => paginationParams.IsDescending 
                    ? query.OrderByDescending(u => u.Role) 
                    : query.OrderBy(u => u.Role),
                _ => paginationParams.IsDescending 
                    ? query.OrderByDescending(u => u.CreatedAt) 
                    : query.OrderBy(u => u.CreatedAt)
            };

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<UserDto>
            {
                Items = _mapper.Map<List<UserDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
        {
            var user = _mapper.Map<User>(createUserDto);
            user.PasswordHash = PasswordHasher.HashPassword(createUserDto.Password);

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User created: {Email}", user.Email);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return null;

            _mapper.Map(updateUserDto, user);
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User updated: {Email}", user.Email);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User deactivated: {Email}", user.Email);
            return true;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _unitOfWork.Repository<User>()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
