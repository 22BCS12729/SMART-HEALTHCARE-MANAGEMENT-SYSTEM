using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Services
{
    public class SpecializationService : ISpecializationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SpecializationService> _logger;

        public SpecializationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SpecializationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SpecializationDto?> GetByIdAsync(int id)
        {
            var spec = await _unitOfWork.Repository<Specialization>().Query()
                .Include(s => s.DoctorSpecializations)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            return spec != null ? _mapper.Map<SpecializationDto>(spec) : null;
        }

        public async Task<IEnumerable<SpecializationDto>> GetAllAsync()
        {
            var specs = await _unitOfWork.Repository<Specialization>().Query()
                .Include(s => s.DoctorSpecializations)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return _mapper.Map<List<SpecializationDto>>(specs);
        }

        public async Task<SpecializationWithDoctorsDto?> GetWithDoctorsAsync(int id)
        {
            var spec = await _unitOfWork.Repository<Specialization>().Query()
                .Include(s => s.DoctorSpecializations)
                    .ThenInclude(ds => ds.Doctor)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (spec == null) return null;

            var result = _mapper.Map<SpecializationWithDoctorsDto>(spec);
            result.Doctors = _mapper.Map<List<DoctorListDto>>(spec.DoctorSpecializations.Select(ds => ds.Doctor));
            return result;
        }

        public async Task<SpecializationDto> CreateAsync(CreateSpecializationDto createSpecializationDto)
        {
            var spec = _mapper.Map<Specialization>(createSpecializationDto);
            await _unitOfWork.Repository<Specialization>().AddAsync(spec);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Specialization created: {Name}", spec.Name);
            return _mapper.Map<SpecializationDto>(spec);
        }

        public async Task<SpecializationDto?> UpdateAsync(int id, UpdateSpecializationDto updateSpecializationDto)
        {
            var spec = await _unitOfWork.Repository<Specialization>().GetByIdAsync(id);
            if (spec == null || !spec.IsActive) return null;

            _mapper.Map(updateSpecializationDto, spec);
            _unitOfWork.Repository<Specialization>().Update(spec);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Specialization {Id} updated", id);
            return _mapper.Map<SpecializationDto>(spec);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var spec = await _unitOfWork.Repository<Specialization>().GetByIdAsync(id);
            if (spec == null) return false;

            // Check if any doctors are using this specialization
            var hasDoctors = await _unitOfWork.Repository<DoctorSpecialization>()
                .AnyAsync(ds => ds.SpecializationId == id);

            if (hasDoctors)
            {
                throw new InvalidOperationException("Cannot delete specialization with assigned doctors");
            }

            spec.IsActive = false;
            _unitOfWork.Repository<Specialization>().Update(spec);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Specialization {Id} deactivated", id);
            return true;
        }
    }
}
