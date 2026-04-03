using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicineService> _logger;

        public MedicineService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MedicineService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MedicineDto?> GetByIdAsync(int id)
        {
            var medicine = await _unitOfWork.Repository<Medicine>().GetByIdAsync(id);
            return medicine != null && medicine.IsActive ? _mapper.Map<MedicineDto>(medicine) : null;
        }

        public async Task<PagedResultDto<MedicineDto>> GetAllAsync(PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<Medicine>().Query()
                .Where(m => m.IsActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.Name)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<MedicineDto>
            {
                Items = _mapper.Map<List<MedicineDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<PagedResultDto<MedicineListDto>> SearchAsync(MedicineSearchDto searchDto, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<Medicine>().Query()
                .Where(m => m.IsActive);

            // Apply search filters
            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                var term = searchDto.SearchTerm.ToLower();
                query = query.Where(m => 
                    m.Name.ToLower().Contains(term) ||
                    (m.GenericName != null && m.GenericName.ToLower().Contains(term)) ||
                    (m.Description != null && m.Description.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Manufacturer))
            {
                query = query.Where(m => m.Manufacturer != null && m.Manufacturer.Contains(searchDto.Manufacturer));
            }

            if (!string.IsNullOrWhiteSpace(searchDto.DosageForm))
            {
                query = query.Where(m => m.DosageForm == searchDto.DosageForm);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.Name)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<MedicineListDto>
            {
                Items = _mapper.Map<List<MedicineListDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<MedicineDto> CreateAsync(CreateMedicineDto createMedicineDto)
        {
            var medicine = _mapper.Map<Medicine>(createMedicineDto);
            await _unitOfWork.Repository<Medicine>().AddAsync(medicine);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine created: {Name}", medicine.Name);
            return _mapper.Map<MedicineDto>(medicine);
        }

        public async Task<MedicineDto?> UpdateAsync(int id, UpdateMedicineDto updateMedicineDto)
        {
            var medicine = await _unitOfWork.Repository<Medicine>().GetByIdAsync(id);
            if (medicine == null || !medicine.IsActive) return null;

            _mapper.Map(updateMedicineDto, medicine);
            _unitOfWork.Repository<Medicine>().Update(medicine);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine {Id} updated", id);
            return _mapper.Map<MedicineDto>(medicine);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var medicine = await _unitOfWork.Repository<Medicine>().GetByIdAsync(id);
            if (medicine == null) return false;

            medicine.IsActive = false;
            _unitOfWork.Repository<Medicine>().Update(medicine);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine {Id} deactivated", id);
            return true;
        }
    }
}
