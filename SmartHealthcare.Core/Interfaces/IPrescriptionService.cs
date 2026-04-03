using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.Core.Interfaces
{
    public interface IPrescriptionService
    {
        Task<PrescriptionDto?> GetByIdAsync(int id);
        Task<PrescriptionDto?> GetByAppointmentIdAsync(int appointmentId);
        Task<PagedResultDto<PrescriptionListDto>> GetByPatientAsync(int patientId, PaginationParams paginationParams);
        Task<PagedResultDto<PrescriptionListDto>> GetByDoctorAsync(int doctorId, PaginationParams paginationParams);
        Task<PrescriptionDto> CreateAsync(int doctorId, CreatePrescriptionDto createPrescriptionDto);
        Task<PrescriptionDto?> UpdateAsync(int id, UpdatePrescriptionDto updatePrescriptionDto);
        Task<bool> DeleteAsync(int id);
    }
}
