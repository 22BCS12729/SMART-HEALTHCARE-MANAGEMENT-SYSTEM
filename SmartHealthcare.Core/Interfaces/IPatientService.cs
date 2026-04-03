using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.Core.Interfaces
{
    public interface IPatientService
    {
        Task<PatientDto?> GetByIdAsync(int id);
        Task<PatientDto?> GetByUserIdAsync(int userId);
        Task<PatientProfileDto?> GetProfileAsync(int id);
        Task<PagedResultDto<PatientDto>> GetAllAsync(PaginationParams paginationParams);
        Task<PatientDto> CreateAsync(CreatePatientDto createPatientDto);
        Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto updatePatientDto);
        Task<bool> DeleteAsync(int id);
    }
}
