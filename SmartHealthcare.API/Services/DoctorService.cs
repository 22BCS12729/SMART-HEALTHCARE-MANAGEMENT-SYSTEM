using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.API.Helpers;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Enums;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DoctorService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DoctorDto?> GetByIdAsync(int id)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().Query()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            return doctor != null ? _mapper.Map<DoctorDto>(doctor) : null;
        }

        public async Task<DoctorDto?> GetByUserIdAsync(int userId)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().Query()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.IsActive);

            return doctor != null ? _mapper.Map<DoctorDto>(doctor) : null;
        }

        public async Task<PagedResultDto<DoctorDto>> GetAllAsync(PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<Doctor>().Query()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Where(d => d.IsActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<DoctorDto>
            {
                Items = _mapper.Map<List<DoctorDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<PagedResultDto<DoctorListDto>> SearchAsync(string? specialization, string? searchTerm, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<Doctor>().Query()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Where(d => d.IsActive);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(d => 
                    d.User.FirstName.ToLower().Contains(term) ||
                    d.User.LastName.ToLower().Contains(term) ||
                    d.User.Email.ToLower().Contains(term) ||
                    (d.Biography != null && d.Biography.ToLower().Contains(term)));
            }

            // Apply specialization filter
            if (!string.IsNullOrWhiteSpace(specialization))
            {
                var spec = specialization.ToLower();
                query = query.Where(d => 
                    d.DoctorSpecializations.Any(ds => ds.Specialization.Name.ToLower() == spec));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<DoctorListDto>
            {
                Items = _mapper.Map<List<DoctorListDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<IEnumerable<DoctorListDto>> GetBySpecializationAsync(int specializationId)
        {
            var doctors = await _unitOfWork.Repository<Doctor>().Query()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Where(d => d.IsActive && d.DoctorSpecializations.Any(ds => ds.SpecializationId == specializationId))
                .ToListAsync();

            return _mapper.Map<List<DoctorListDto>>(doctors);
        }

        public async Task<DoctorDto> CreateAsync(CreateDoctorDto createDoctorDto)
        {
            // Create User first
            var user = new User
            {
                FirstName = createDoctorDto.FirstName,
                LastName = createDoctorDto.LastName,
                Email = createDoctorDto.Email,
                PasswordHash = PasswordHasher.HashPassword(createDoctorDto.Password),
                PhoneNumber = createDoctorDto.PhoneNumber,
                Role = UserRole.Doctor
            };

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Create Doctor profile
            var doctor = _mapper.Map<Doctor>(createDoctorDto);
            doctor.UserId = user.Id;

            await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            // Add specializations
            if (createDoctorDto.SpecializationIds?.Any() == true)
            {
                foreach (var specId in createDoctorDto.SpecializationIds)
                {
                    await AssignSpecializationAsync(doctor.Id, specId);
                }
            }

            _logger.LogInformation("Doctor created: {Email}", user.Email);
            
            // Reload with specializations
            return await GetByIdAsync(doctor.Id) ?? _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<DoctorDto?> UpdateAsync(int id, UpdateDoctorDto updateDoctorDto)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().Query()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (doctor == null) return null;

            _mapper.Map(updateDoctorDto, doctor);
            
            // Update user phone if provided
            if (!string.IsNullOrEmpty(updateDoctorDto.PhoneNumber))
            {
                doctor.User.PhoneNumber = updateDoctorDto.PhoneNumber;
                _unitOfWork.Repository<User>().Update(doctor.User);
            }

            // Update specializations if provided
            if (updateDoctorDto.SpecializationIds != null)
            {
                // Remove existing specializations
                var existing = await _unitOfWork.Repository<DoctorSpecialization>()
                    .FindAsync(ds => ds.DoctorId == id);
                _unitOfWork.Repository<DoctorSpecialization>().RemoveRange(existing);

                // Add new specializations
                foreach (var specId in updateDoctorDto.SpecializationIds)
                {
                    var ds = new DoctorSpecialization
                    {
                        DoctorId = id,
                        SpecializationId = specId
                    };
                    await _unitOfWork.Repository<DoctorSpecialization>().AddAsync(ds);
                }
            }

            _unitOfWork.Repository<Doctor>().Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Doctor updated: {Id}", id);
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id);
            if (doctor == null) return false;

            doctor.IsActive = false;
            _unitOfWork.Repository<Doctor>().Update(doctor);

            // Also deactivate the user
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(doctor.UserId);
            if (user != null)
            {
                user.IsActive = false;
                _unitOfWork.Repository<User>().Update(user);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Doctor deactivated: {Id}", id);
            return true;
        }

        public async Task<DoctorScheduleDto?> GetScheduleAsync(int doctorId, DateTime? date = null)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().Query()
                .Include(d => d.User)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.IsActive);

            if (doctor == null) return null;

            var targetDate = date ?? DateTime.Now.Date;
            var appointments = doctor.Appointments
                .Where(a => a.IsActive && a.AppointmentDate.Date == targetDate.Date)
                .OrderBy(a => a.AppointmentTime)
                .ToList();

            return new DoctorScheduleDto
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                AvailableDays = doctor.AvailableDays,
                AvailableFrom = doctor.AvailableFrom,
                AvailableTo = doctor.AvailableTo,
                BookedSlots = appointments.Select(a => new AppointmentSlotDto
                {
                    Date = a.AppointmentDate,
                    Time = a.AppointmentTime,
                    IsAvailable = false
                }).ToList()
            };
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

        public async Task<bool> AssignSpecializationAsync(int doctorId, int specializationId)
        {
            var exists = await _unitOfWork.Repository<DoctorSpecialization>()
                .AnyAsync(ds => ds.DoctorId == doctorId && ds.SpecializationId == specializationId);

            if (exists) return true;

            var ds = new DoctorSpecialization
            {
                DoctorId = doctorId,
                SpecializationId = specializationId
            };

            await _unitOfWork.Repository<DoctorSpecialization>().AddAsync(ds);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Specialization {SpecializationId} assigned to doctor {DoctorId}", specializationId, doctorId);
            return true;
        }

        public async Task<bool> RemoveSpecializationAsync(int doctorId, int specializationId)
        {
            var ds = await _unitOfWork.Repository<DoctorSpecialization>()
                .FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.SpecializationId == specializationId);

            if (ds == null) return false;

            _unitOfWork.Repository<DoctorSpecialization>().Remove(ds);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Specialization {SpecializationId} removed from doctor {DoctorId}", specializationId, doctorId);
            return true;
        }
    }
}
