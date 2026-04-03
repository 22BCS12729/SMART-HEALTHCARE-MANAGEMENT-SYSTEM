using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Enums;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDoctorService doctorService,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _doctorService = doctorService;
            _logger = logger;
        }

        /// <summary>
        /// Get all appointments (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResultDto<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<AppointmentDto>>> GetAll(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] AppointmentFilterDto? filter)
        {
            var appointments = await _appointmentService.GetAllAsync(filter, paginationParams);
            return Ok(appointments);
        }

        /// <summary>
        /// Get appointment by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentDto>> GetById(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            // Check authorization
            if (!await IsAuthorizedForAppointment(appointment))
            {
                return Forbid();
            }

            return Ok(appointment);
        }

        /// <summary>
        /// Get my appointments (for authenticated patient or doctor)
        /// </summary>
        [HttpGet("my-appointments")]
        [Authorize(Roles = "Patient,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetMyAppointments([FromQuery] AppointmentStatus? status)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role == "Patient")
            {
                var patient = await _patientService.GetByUserIdAsync(userId);
                if (patient == null)
                {
                    return NotFound(new { message = "Patient profile not found" });
                }
                var appointments = await _appointmentService.GetByPatientAsync(patient.Id, status);
                return Ok(appointments);
            }
            else // Doctor
            {
                var doctor = await _doctorService.GetByUserIdAsync(userId);
                if (doctor == null)
                {
                    return NotFound(new { message = "Doctor profile not found" });
                }
                var appointments = await _appointmentService.GetByDoctorAsync(doctor.Id);
                if (status.HasValue)
                {
                    appointments = appointments.Where(a => a.Status == status.Value);
                }
                return Ok(appointments);
            }
        }

        /// <summary>
        /// Get today's appointments for doctor
        /// </summary>
        [HttpGet("today")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetTodayAppointments()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor profile not found" });
            }

            var appointments = await _appointmentService.GetByDoctorAsync(doctor.Id, DateTime.Now);
            return Ok(appointments);
        }

        /// <summary>
        /// Create new appointment (Patient or Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Patient")]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentDto createAppointmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var userEmail = User.FindFirst("email")?.Value ?? "";
            var userName = User.FindFirst("given_name")?.Value ?? "Admin";
            var userLastName = User.FindFirst("family_name")?.Value ?? "User";
            
            var patient = await _patientService.GetByUserIdAsync(userId);

            // If user is Patient, they must have a patient profile
            if (User.IsInRole("Patient") && patient == null)
            {
                return NotFound(new { message = "Patient profile not found" });
            }

            // If admin doesn't have patient profile, auto-create one
            int patientId;
            if (patient == null)
            {
                // Auto-create patient profile for admin
                var createPatientDto = new CreatePatientDto
                {
                    FirstName = userName,
                    LastName = userLastName,
                    Email = userEmail,
                    Password = "Temp@123", // Will be changed immediately
                    PhoneNumber = "0000000000",
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    Gender = Gender.Male,
                    Address = "Auto-created for admin"
                };
                
                patient = await _patientService.CreateAsync(createPatientDto);
                _logger.LogInformation("Auto-created patient profile for admin user {UserId}", userId);
            }
            
            patientId = patient.Id;

            try
            {
                var appointment = await _appointmentService.CreateAsync(patientId, createAppointmentDto);
                return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Book appointment with validation (Patient or Admin)
        /// </summary>
        [HttpPost("book")]
        [Authorize(Roles = "Admin,Patient")]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppointmentDto>> BookAppointment([FromBody] BookAppointmentRequestDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var patient = await _patientService.GetByUserIdAsync(userId);

            if (patient == null)
            {
                return NotFound(new { message = "Patient profile not found" });
            }

            // Check if slot is available
            var availableSlots = await _appointmentService.GetAvailableSlotsAsync(bookDto.DoctorId, bookDto.AppointmentDate);
            if (!availableSlots.Any(s => s.Time == bookDto.AppointmentTime && s.IsAvailable))
            {
                return BadRequest(new { message = "Selected time slot is not available" });
            }

            var createDto = new CreateAppointmentDto
            {
                DoctorId = bookDto.DoctorId,
                AppointmentDate = bookDto.AppointmentDate,
                AppointmentTime = bookDto.AppointmentTime,
                Reason = bookDto.Reason
            };

            try
            {
                var appointment = await _appointmentService.CreateAsync(patient.Id, createDto);
                return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update appointment
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Patient")]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppointmentDto>> Update(int id, [FromBody] UpdateAppointmentDto updateAppointmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            // Check authorization for patients
            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var patient = await _patientService.GetByUserIdAsync(userId);
                if (patient == null || appointment.PatientId != patient.Id)
                {
                    return Forbid();
                }

                // Patients can only update notes, not time/date if confirmed
                if (appointment.Status == AppointmentStatus.Confirmed && 
                    (updateAppointmentDto.AppointmentDate.HasValue || updateAppointmentDto.AppointmentTime.HasValue))
                {
                    return BadRequest(new { message = "Cannot change appointment time after confirmation" });
                }
            }

            var updated = await _appointmentService.UpdateAsync(id, updateAppointmentDto);
            return Ok(updated);
        }

        /// <summary>
        /// Update appointment status (Doctor or Admin)
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentDto>> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            // Check authorization for doctors
            if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var doctor = await _doctorService.GetByUserIdAsync(userId);
                if (doctor == null || appointment.DoctorId != doctor.Id)
                {
                    return Forbid();
                }
            }

            var updated = await _appointmentService.UpdateStatusAsync(id, statusDto.Status, statusDto.Notes);
            return Ok(updated);
        }

        /// <summary>
        /// Cancel appointment (Patient can cancel their own)
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        [Authorize(Roles = "Admin,Patient")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelAppointment(int id, [FromBody] string? reason)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            // Check authorization for patients
            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var patient = await _patientService.GetByUserIdAsync(userId);
                if (patient == null || appointment.PatientId != patient.Id)
                {
                    return Forbid();
                }
            }

            var updated = await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.Cancelled, $"Cancelled: {reason}");
            return Ok(new { message = "Appointment cancelled successfully", appointment = updated });
        }

        /// <summary>
        /// Delete appointment (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _appointmentService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            return NoContent();
        }

        /// <summary>
        /// Get appointment statistics (Admin only)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AppointmentStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppointmentStatsDto>> GetStats()
        {
            var stats = await _appointmentService.GetStatsAsync();
            return Ok(stats);
        }

        private async Task<bool> IsAuthorizedForAppointment(AppointmentDto appointment)
        {
            if (User.IsInRole("Admin"))
                return true;

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role == "Patient")
            {
                var patient = await _patientService.GetByUserIdAsync(userId);
                return patient != null && appointment.PatientId == patient.Id;
            }
            else if (role == "Doctor")
            {
                var doctor = await _doctorService.GetByUserIdAsync(userId);
                return doctor != null && appointment.DoctorId == doctor.Id;
            }

            return false;
        }
    }
}
