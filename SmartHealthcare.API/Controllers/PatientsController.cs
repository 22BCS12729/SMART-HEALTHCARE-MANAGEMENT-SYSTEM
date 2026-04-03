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
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IPatientService patientService, IAppointmentService appointmentService, ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all patients (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResultDto<PatientDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<PatientDto>>> GetAll([FromQuery] PaginationParams paginationParams)
        {
            var patients = await _patientService.GetAllAsync(paginationParams);
            return Ok(patients);
        }

        /// <summary>
        /// Get patient by ID (Admin, Doctor, or own patient)
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientDto>> GetById(int id)
        {
            // Check authorization for patients
            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var patient = await _patientService.GetByIdAsync(id);
                if (patient == null || patient.UserId != userId)
                {
                    return Forbid();
                }
            }

            var result = await _patientService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Patient not found" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get patient profile with appointments
        /// </summary>
        [HttpGet("profile")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(PatientProfileDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<PatientProfileDto>> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var patient = await _patientService.GetByUserIdAsync(userId);

            if (patient == null)
            {
                return NotFound(new { message = "Patient profile not found" });
            }

            var profile = await _patientService.GetProfileAsync(patient.Id);
            return Ok(profile);
        }

        /// <summary>
        /// Create new patient (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PatientDto>> Create([FromBody] CreatePatientDto createPatientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var patient = await _patientService.CreateAsync(createPatientDto);
                return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update patient (Admin or own patient)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Patient")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PatientDto>> Update(int id, [FromBody] UpdatePatientDto updatePatientDto)
        {
            // Check authorization for patients
            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var existingPatient = await _patientService.GetByIdAsync(id);
                if (existingPatient == null || existingPatient.UserId != userId)
                {
                    return Forbid();
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var patient = await _patientService.UpdateAsync(id, updatePatientDto);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found" });
            }

            return Ok(patient);
        }

        /// <summary>
        /// Delete patient (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _patientService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Patient not found" });
            }

            return NoContent();
        }

        /// <summary>
        /// Get patient's appointments
        /// </summary>
        [HttpGet("{id:int}/appointments")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetPatientAppointments(int id, [FromQuery] AppointmentStatus? status)
        {
            // Check authorization for patients
            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var patient = await _patientService.GetByIdAsync(id);
                if (patient == null || patient.UserId != userId)
                {
                    return Forbid();
                }
            }

            var appointments = await _appointmentService.GetByPatientAsync(id, status);
            return Ok(appointments);
        }
    }
}
