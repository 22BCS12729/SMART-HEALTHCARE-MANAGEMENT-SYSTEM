using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(
            IPrescriptionService prescriptionService,
            IDoctorService doctorService,
            IPatientService patientService,
            ILogger<PrescriptionsController> logger)
        {
            _prescriptionService = prescriptionService;
            _doctorService = doctorService;
            _patientService = patientService;
            _logger = logger;
        }

        /// <summary>
        /// Get prescription by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionDto>> GetById(int id)
        {
            var prescription = await _prescriptionService.GetByIdAsync(id);
            if (prescription == null)
            {
                return NotFound(new { message = "Prescription not found" });
            }

            // Check authorization for patients
            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var patient = await _patientService.GetByUserIdAsync(userId);
                if (patient == null || prescription.PatientId != patient.Id)
                {
                    return Forbid();
                }
            }

            return Ok(prescription);
        }

        /// <summary>
        /// Get prescription by appointment ID
        /// </summary>
        [HttpGet("by-appointment/{appointmentId:int}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionDto>> GetByAppointmentId(int appointmentId)
        {
            var prescription = await _prescriptionService.GetByAppointmentIdAsync(appointmentId);
            if (prescription == null)
            {
                return NotFound(new { message = "Prescription not found" });
            }

            // Check authorization for patients
            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var patient = await _patientService.GetByUserIdAsync(userId);
                if (patient == null || prescription.PatientId != patient.Id)
                {
                    return Forbid();
                }
            }

            return Ok(prescription);
        }

        /// <summary>
        /// Get my prescriptions (for authenticated patient or doctor)
        /// </summary>
        [HttpGet("my-prescriptions")]
        [Authorize(Roles = "Patient,Doctor")]
        [ProducesResponseType(typeof(PagedResultDto<PrescriptionListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<PrescriptionListDto>>> GetMyPrescriptions([FromQuery] PaginationParams paginationParams)
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
                var prescriptions = await _prescriptionService.GetByPatientAsync(patient.Id, paginationParams);
                return Ok(prescriptions);
            }
            else // Doctor
            {
                var doctor = await _doctorService.GetByUserIdAsync(userId);
                if (doctor == null)
                {
                    return NotFound(new { message = "Doctor profile not found" });
                }
                var prescriptions = await _prescriptionService.GetByDoctorAsync(doctor.Id, paginationParams);
                return Ok(prescriptions);
            }
        }

        /// <summary>
        /// Create new prescription (Doctor only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionDto>> Create([FromBody] CreatePrescriptionDto createPrescriptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var doctor = await _doctorService.GetByUserIdAsync(userId);

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor profile not found" });
            }

            try
            {
                var prescription = await _prescriptionService.CreateAsync(doctor.Id, createPrescriptionDto);
                return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update prescription (Doctor who created it only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PrescriptionDto>> Update(int id, [FromBody] UpdatePrescriptionDto updatePrescriptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _prescriptionService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Prescription not found" });
            }

            // Check if the doctor created this prescription
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor == null || existing.DoctorId != doctor.Id)
            {
                return Forbid();
            }

            var updated = await _prescriptionService.UpdateAsync(id, updatePrescriptionDto);
            return Ok(updated);
        }

        /// <summary>
        /// Delete prescription (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _prescriptionService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Prescription not found" });
            }

            return NoContent();
        }
    }
}
