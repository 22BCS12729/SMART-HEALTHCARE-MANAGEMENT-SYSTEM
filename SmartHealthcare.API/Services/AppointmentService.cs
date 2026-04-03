using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Enums;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AppointmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AppointmentDto?> GetByIdAsync(int id)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Prescription)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

            return appointment != null ? _mapper.Map<AppointmentDto>(appointment) : null;
        }

        public async Task<PagedResultDto<AppointmentDto>> GetAllAsync(AppointmentFilterDto? filter, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<Appointment>().Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Prescription)
                .Where(a => a.IsActive);

            // Apply filters
            if (filter?.PatientId.HasValue == true)
                query = query.Where(a => a.PatientId == filter.PatientId.Value);

            if (filter?.DoctorId.HasValue == true)
                query = query.Where(a => a.DoctorId == filter.DoctorId.Value);

            if (filter?.FromDate.HasValue == true)
                query = query.Where(a => a.AppointmentDate >= filter.FromDate.Value);

            if (filter?.ToDate.HasValue == true)
                query = query.Where(a => a.AppointmentDate <= filter.ToDate.Value);

            if (filter?.Status.HasValue == true)
                query = query.Where(a => a.Status == filter.Status.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<AppointmentDto>
            {
                Items = _mapper.Map<List<AppointmentDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<IEnumerable<AppointmentDto>> GetByPatientAsync(int patientId, AppointmentStatus? status = null)
        {
            var query = _unitOfWork.Repository<Appointment>().Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Prescription)
                .Where(a => a.PatientId == patientId && a.IsActive);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return _mapper.Map<List<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(int doctorId, DateTime? date = null)
        {
            var query = _unitOfWork.Repository<Appointment>().Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Prescription)
                .Where(a => a.DoctorId == doctorId && a.IsActive);

            if (date.HasValue)
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return _mapper.Map<List<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);
            if (doctor == null) return new List<AppointmentSlotDto>();

            var existingAppointments = await _unitOfWork.Repository<Appointment>().Query()
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date && a.IsActive)
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            var slots = new List<AppointmentSlotDto>();
            
            if (doctor.AvailableFrom.HasValue && doctor.AvailableTo.HasValue)
            {
                var currentTime = doctor.AvailableFrom.Value;
                while (currentTime < doctor.AvailableTo.Value)
                {
                    slots.Add(new AppointmentSlotDto
                    {
                        Date = date,
                        Time = currentTime,
                        IsAvailable = !existingAppointments.Contains(currentTime)
                    });
                    currentTime = currentTime.Add(TimeSpan.FromMinutes(30));
                }
            }

            return slots;
        }

        public async Task<AppointmentDto> CreateAsync(int patientId, CreateAppointmentDto createAppointmentDto)
        {
            var appointment = _mapper.Map<Appointment>(createAppointmentDto);
            appointment.PatientId = patientId;

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Appointment created for patient {PatientId} with doctor {DoctorId} on {Date}",
                patientId, createAppointmentDto.DoctorId, createAppointmentDto.AppointmentDate);

            return await GetByIdAsync(appointment.Id) ?? _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment == null || !appointment.IsActive) return null;

            _mapper.Map(updateAppointmentDto, appointment);
            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Appointment {Id} updated", id);
            return await GetByIdAsync(id);
        }

        public async Task<AppointmentDto?> UpdateStatusAsync(int id, AppointmentStatus status, string? notes = null)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment == null || !appointment.IsActive) return null;

            appointment.Status = status;
            if (!string.IsNullOrEmpty(notes))
                appointment.Notes = notes;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Appointment {Id} status updated to {Status}", id, status);
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment == null) return false;

            appointment.IsActive = false;
            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Appointment {Id} cancelled", id);
            return true;
        }

        public async Task<AppointmentStatsDto> GetStatsAsync()
        {
            var today = DateTime.Now.Date;

            var total = await _unitOfWork.Repository<Appointment>().CountAsync(a => a.IsActive);
            var pending = await _unitOfWork.Repository<Appointment>().CountAsync(a => a.IsActive && a.Status == AppointmentStatus.Pending);
            var confirmed = await _unitOfWork.Repository<Appointment>().CountAsync(a => a.IsActive && a.Status == AppointmentStatus.Confirmed);
            var completed = await _unitOfWork.Repository<Appointment>().CountAsync(a => a.IsActive && a.Status == AppointmentStatus.Completed);
            var cancelled = await _unitOfWork.Repository<Appointment>().CountAsync(a => a.IsActive && a.Status == AppointmentStatus.Cancelled);
            var todayCount = await _unitOfWork.Repository<Appointment>().CountAsync(a => a.IsActive && a.AppointmentDate.Date == today);

            return new AppointmentStatsDto
            {
                TotalAppointments = total,
                PendingAppointments = pending,
                ConfirmedAppointments = confirmed,
                CompletedAppointments = completed,
                CancelledAppointments = cancelled,
                TodayAppointments = todayCount
            };
        }
    }
}
