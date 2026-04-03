using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.Core.Interfaces
{
    public interface IDoctorService
    {
        Task<DoctorDto?> GetByIdAsync(int id);
        Task<DoctorDto?> GetByUserIdAsync(int userId);
        Task<PagedResultDto<DoctorDto>> GetAllAsync(PaginationParams paginationParams);
        Task<PagedResultDto<DoctorListDto>> SearchAsync(string? specialization, string? searchTerm, PaginationParams paginationParams);
        Task<IEnumerable<DoctorListDto>> GetBySpecializationAsync(int specializationId);
        Task<DoctorDto> CreateAsync(CreateDoctorDto createDoctorDto);
        Task<DoctorDto?> UpdateAsync(int id, UpdateDoctorDto updateDoctorDto);
        Task<bool> DeleteAsync(int id);
        Task<DoctorScheduleDto?> GetScheduleAsync(int doctorId, DateTime? date = null);
        Task<IEnumerable<AppointmentSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date);
        Task<bool> AssignSpecializationAsync(int doctorId, int specializationId);
        Task<bool> RemoveSpecializationAsync(int doctorId, int specializationId);
    }
}
