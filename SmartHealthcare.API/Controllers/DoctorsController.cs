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
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorService doctorService, IAppointmentService appointmentService, ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all doctors
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResultDto<DoctorDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<DoctorDto>>> GetAll([FromQuery] PaginationParams paginationParams)
        {
            var doctors = await _doctorService.GetAllAsync(paginationParams);
            return Ok(doctors);
        }

        /// <summary>
        /// Search doctors by specialization or name
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResultDto<DoctorListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<DoctorListDto>>> Search(
            [FromQuery] string? specialization,
            [FromQuery] string? searchTerm,
            [FromQuery] PaginationParams paginationParams)
        {
            var doctors = await _doctorService.SearchAsync(specialization, searchTerm, paginationParams);
            return Ok(doctors);
        }

        /// <summary>
        /// Get doctor by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorDto>> GetById(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            return Ok(doctor);
        }

        /// <summary>
        /// Get doctor profile (for authenticated doctor)
        /// </summary>
        [HttpGet("profile")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DoctorDto>> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var doctor = await _doctorService.GetByUserIdAsync(userId);

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor profile not found" });
            }

            return Ok(doctor);
        }

        /// <summary>
        /// Get doctors by specialization
        /// </summary>
        [HttpGet("by-specialization/{specializationId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<DoctorListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DoctorListDto>>> GetBySpecialization(int specializationId)
        {
            var doctors = await _doctorService.GetBySpecializationAsync(specializationId);
            return Ok(doctors);
        }

        /// <summary>
        /// Get doctor schedule with available/booked slots
        /// </summary>
        [HttpGet("{id:int}/schedule")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DoctorScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorScheduleDto>> GetSchedule(int id, [FromQuery] DateTime? date)
        {
            var schedule = await _doctorService.GetScheduleAsync(id, date);
            if (schedule == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            return Ok(schedule);
        }

        /// <summary>
        /// Get available time slots for a doctor on a specific date
        /// </summary>
        [HttpGet("{id:int}/available-slots")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<AppointmentSlotDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppointmentSlotDto>>> GetAvailableSlots(int id, [FromQuery] DateTime date)
        {
            var slots = await _doctorService.GetAvailableSlotsAsync(id, date);
            return Ok(slots);
        }

        /// <summary>
        /// Create new doctor (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DoctorDto>> Create([FromBody] CreateDoctorDto createDoctorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var doctor = await _doctorService.CreateAsync(createDoctorDto);
                return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update doctor (Admin or own doctor)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DoctorDto>> Update(int id, [FromBody] UpdateDoctorDto updateDoctorDto)
        {
            // Check authorization for doctors
            if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var existingDoctor = await _doctorService.GetByIdAsync(id);
                if (existingDoctor == null || existingDoctor.UserId != userId)
                {
                    return Forbid();
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var doctor = await _doctorService.UpdateAsync(id, updateDoctorDto);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            return Ok(doctor);
        }

        /// <summary>
        /// Partial update doctor (PATCH)
        /// </summary>
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorDto>> Patch(int id, [FromBody] UpdateDoctorDto updateDoctorDto)
        {
            return await Update(id, updateDoctorDto);
        }

        /// <summary>
        /// Delete doctor (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _doctorService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            return NoContent();
        }

        /// <summary>
        /// Assign specialization to doctor (Admin only)
        /// </summary>
        [HttpPost("{id:int}/specializations")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignSpecialization(int id, [FromBody] AssignSpecializationDto dto)
        {
            var result = await _doctorService.AssignSpecializationAsync(id, dto.SpecializationId);
            if (!result)
            {
                return NotFound(new { message = "Doctor or specialization not found" });
            }

            return Ok(new { message = "Specialization assigned successfully" });
        }

        /// <summary>
        /// Remove specialization from doctor (Admin only)
        /// </summary>
        [HttpDelete("{id:int}/specializations/{specializationId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveSpecialization(int id, int specializationId)
        {
            var result = await _doctorService.RemoveSpecializationAsync(id, specializationId);
            if (!result)
            {
                return NotFound(new { message = "Specialization assignment not found" });
            }

            return Ok(new { message = "Specialization removed successfully" });
        }

        /// <summary>
        /// Get doctor's appointments
        /// </summary>
        [HttpGet("{id:int}/appointments")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetDoctorAppointments(int id, [FromQuery] DateTime? date)
        {
            // Check authorization for doctors
            if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var doctor = await _doctorService.GetByIdAsync(id);
                if (doctor == null || doctor.UserId != userId)
                {
                    return Forbid();
                }
            }

            var appointments = await _appointmentService.GetByDoctorAsync(id, date);
            return Ok(appointments);
        }
    }
}
