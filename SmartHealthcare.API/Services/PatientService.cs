using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.API.Helpers;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Enums;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PatientService> _logger;

        public PatientService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PatientService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PatientDto?> GetByIdAsync(int id)
        {
            var patient = await _unitOfWork.Repository<Patient>().Query()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return patient != null ? _mapper.Map<PatientDto>(patient) : null;
        }

        public async Task<PatientDto?> GetByUserIdAsync(int userId)
        {
            var patient = await _unitOfWork.Repository<Patient>().Query()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

            return patient != null ? _mapper.Map<PatientDto>(patient) : null;
        }

        public async Task<PatientProfileDto?> GetProfileAsync(int id)
        {
            var patient = await _unitOfWork.Repository<Patient>().Query()
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (patient == null) return null;

            var today = DateTime.Now.Date;
            var upcomingAppointments = patient.Appointments
                .Where(a => a.IsActive && a.AppointmentDate >= today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime);

            var pastAppointments = patient.Appointments
                .Where(a => a.IsActive && a.AppointmentDate < today)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime);

            return new PatientProfileDto
            {
                Patient = _mapper.Map<PatientDto>(patient),
                UpcomingAppointments = _mapper.Map<List<AppointmentDto>>(upcomingAppointments),
                PastAppointments = _mapper.Map<List<AppointmentDto>>(pastAppointments)
            };
        }

        public async Task<PagedResultDto<PatientDto>> GetAllAsync(PaginationParams paginationParams)
        {
            var query = _unitOfWork.Repository<Patient>().Query()
                .Include(p => p.User)
                .Where(p => p.IsActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<PatientDto>
            {
                Items = _mapper.Map<List<PatientDto>>(items),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<PatientDto> CreateAsync(CreatePatientDto createPatientDto)
        {
            // Create User first
            var user = new User
            {
                FirstName = createPatientDto.FirstName,
                LastName = createPatientDto.LastName,
                Email = createPatientDto.Email,
                PasswordHash = PasswordHasher.HashPassword(createPatientDto.Password),
                PhoneNumber = createPatientDto.PhoneNumber,
                Role = UserRole.Patient
            };

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Create Patient profile
            var patient = _mapper.Map<Patient>(createPatientDto);
            patient.UserId = user.Id;

            await _unitOfWork.Repository<Patient>().AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Patient created: {Email}", user.Email);
            return _mapper.Map<PatientDto>(patient);
        }

        public async Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto updatePatientDto)
        {
            var patient = await _unitOfWork.Repository<Patient>().Query()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (patient == null) return null;

            _mapper.Map(updatePatientDto, patient);
            
            // Update user phone if provided
            if (!string.IsNullOrEmpty(updatePatientDto.PhoneNumber))
            {
                patient.User.PhoneNumber = updatePatientDto.PhoneNumber;
                _unitOfWork.Repository<User>().Update(patient.User);
            }

            _unitOfWork.Repository<Patient>().Update(patient);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Patient updated: {Id}", id);
            return _mapper.Map<PatientDto>(patient);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(id);
            if (patient == null) return false;

            // Soft delete
            patient.IsActive = false;
            _unitOfWork.Repository<Patient>().Update(patient);

            // Also deactivate the user
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(patient.UserId);
            if (user != null)
            {
                user.IsActive = false;
                _unitOfWork.Repository<User>().Update(user);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Patient deactivated: {Id}", id);
            return true;
        }
    }
}
