using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.Core.Interfaces
{
    public interface ISpecializationService
    {
        Task<SpecializationDto?> GetByIdAsync(int id);
        Task<IEnumerable<SpecializationDto>> GetAllAsync();
        Task<SpecializationWithDoctorsDto?> GetWithDoctorsAsync(int id);
        Task<SpecializationDto> CreateAsync(CreateSpecializationDto createSpecializationDto);
        Task<SpecializationDto?> UpdateAsync(int id, UpdateSpecializationDto updateSpecializationDto);
        Task<bool> DeleteAsync(int id);
    }
}
