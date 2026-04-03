using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PrescriptionService> _logger;

        public PrescriptionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PrescriptionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PrescriptionDto?> GetByIdAsync(int id)
        {
            var prescription = await _unitOfWork.Repository<Prescription>().Query()
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Include(p => p.Patient).ThenInclude(pat => pat.User)
                .Include(p => p.PrescriptionMedicines).ThenInclude(pm => pm.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return prescription != null ? _mapper.Map<PrescriptionDto>(prescription) : null;
        }

        public async Task<PrescriptionDto?> GetByAppointmentIdAsync(int appointmentId)
        {
            var prescription = await _unitOfWork.Repository<Prescription>().Query()
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Include(p => p.Patient).ThenInclude(pat => pat.User)
                .Include(p => p.PrescriptionMedicines).ThenInclude(pm => pm.Medicine)
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && p.IsActive);

            return prescription != null ? _mapper.Map<PrescriptionDto>(prescription) : null;
        }

        public async Task<PagedResultDto<PrescriptionListDto>> GetByPatientAsync(int patientId, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<Prescription>().Query()
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Include(p => p.Patient).ThenInclude(pat => pat.User)
                .Where(p => p.PatientId == patientId && p.IsActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.PrescriptionDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<PrescriptionListDto>
            {
                Items = _mapper.Map<List<PrescriptionListDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<PagedResultDto<PrescriptionListDto>> GetByDoctorAsync(int doctorId, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<Prescription>().Query()
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Include(p => p.Patient).ThenInclude(pat => pat.User)
                .Where(p => p.DoctorId == doctorId && p.IsActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.PrescriptionDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<PrescriptionListDto>
            {
                Items = _mapper.Map<List<PrescriptionListDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<PrescriptionDto> CreateAsync(int doctorId, CreatePrescriptionDto createPrescriptionDto)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().Query()
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == createPrescriptionDto.AppointmentId && a.IsActive);

            if (appointment == null)
                throw new InvalidOperationException("Appointment not found");

            var prescription = _mapper.Map<Prescription>(createPrescriptionDto);
            prescription.DoctorId = doctorId;
            prescription.PatientId = appointment.PatientId;
            prescription.PrescriptionDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Prescription>().AddAsync(prescription);
            await _unitOfWork.SaveChangesAsync();

            // Add medicines
            if (createPrescriptionDto.Medicines?.Any() == true)
            {
                foreach (var medDto in createPrescriptionDto.Medicines)
                {
                    var pm = _mapper.Map<PrescriptionMedicine>(medDto);
                    pm.PrescriptionId = prescription.Id;
                    await _unitOfWork.Repository<PrescriptionMedicine>().AddAsync(pm);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            // Update appointment status to completed
            appointment.Status = Core.Enums.AppointmentStatus.Completed;
            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Prescription created for appointment {AppointmentId}", createPrescriptionDto.AppointmentId);

            return await GetByIdAsync(prescription.Id) ?? _mapper.Map<PrescriptionDto>(prescription);
        }

        public async Task<PrescriptionDto?> UpdateAsync(int id, UpdatePrescriptionDto updatePrescriptionDto)
        {
            var prescription = await _unitOfWork.Repository<Prescription>().GetByIdAsync(id);
            if (prescription == null || !prescription.IsActive) return null;

            _mapper.Map(updatePrescriptionDto, prescription);
            _unitOfWork.Repository<Prescription>().Update(prescription);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Prescription {Id} updated", id);
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var prescription = await _unitOfWork.Repository<Prescription>().GetByIdAsync(id);
            if (prescription == null) return false;

            prescription.IsActive = false;
            _unitOfWork.Repository<Prescription>().Update(prescription);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Prescription {Id} deactivated", id);
            return true;
        }
    }
}
