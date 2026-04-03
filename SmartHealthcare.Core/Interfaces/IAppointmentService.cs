using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Enums;

namespace SmartHealthcare.Core.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentDto?> GetByIdAsync(int id);
        Task<PagedResultDto<AppointmentDto>> GetAllAsync(AppointmentFilterDto? filter, PaginationParams paginationParams);
        Task<IEnumerable<AppointmentDto>> GetByPatientAsync(int patientId, AppointmentStatus? status = null);
        Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(int doctorId, DateTime? date = null);
        Task<IEnumerable<AppointmentSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date);
        Task<AppointmentDto> CreateAsync(int patientId, CreateAppointmentDto createAppointmentDto);
        Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto updateAppointmentDto);
        Task<AppointmentDto?> UpdateStatusAsync(int id, AppointmentStatus status, string? notes = null);
        Task<bool> DeleteAsync(int id);
        Task<AppointmentStatsDto> GetStatsAsync();
    }
}
