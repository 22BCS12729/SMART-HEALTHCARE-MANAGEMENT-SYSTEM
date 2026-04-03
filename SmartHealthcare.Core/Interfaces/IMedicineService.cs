using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.Core.Interfaces
{
    public interface IMedicineService
    {
        Task<MedicineDto?> GetByIdAsync(int id);
        Task<PagedResultDto<MedicineDto>> GetAllAsync(PaginationParams paginationParams);
        Task<PagedResultDto<MedicineListDto>> SearchAsync(MedicineSearchDto searchDto, PaginationParams paginationParams);
        Task<MedicineDto> CreateAsync(CreateMedicineDto createMedicineDto);
        Task<MedicineDto?> UpdateAsync(int id, UpdateMedicineDto updateMedicineDto);
        Task<bool> DeleteAsync(int id);
    }
}
