using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecializationsController : ControllerBase
    {
        private readonly ISpecializationService _specializationService;
        private readonly ILogger<SpecializationsController> _logger;

        public SpecializationsController(ISpecializationService specializationService, ILogger<SpecializationsController> logger)
        {
            _specializationService = specializationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all specializations
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<SpecializationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SpecializationDto>>> GetAll()
        {
            var specializations = await _specializationService.GetAllAsync();
            return Ok(specializations);
        }

        /// <summary>
        /// Get specialization by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SpecializationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SpecializationDto>> GetById(int id)
        {
            var specialization = await _specializationService.GetByIdAsync(id);
            if (specialization == null)
            {
                return NotFound(new { message = "Specialization not found" });
            }

            return Ok(specialization);
        }

        /// <summary>
        /// Get specialization with doctors
        /// </summary>
        [HttpGet("{id:int}/with-doctors")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SpecializationWithDoctorsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SpecializationWithDoctorsDto>> GetWithDoctors(int id)
        {
            var specialization = await _specializationService.GetWithDoctorsAsync(id);
            if (specialization == null)
            {
                return NotFound(new { message = "Specialization not found" });
            }

            return Ok(specialization);
        }

        /// <summary>
        /// Create new specialization (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SpecializationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SpecializationDto>> Create([FromBody] CreateSpecializationDto createSpecializationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var specialization = await _specializationService.CreateAsync(createSpecializationDto);
                return CreatedAtAction(nameof(GetById), new { id = specialization.Id }, specialization);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update specialization (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SpecializationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SpecializationDto>> Update(int id, [FromBody] UpdateSpecializationDto updateSpecializationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var specialization = await _specializationService.UpdateAsync(id, updateSpecializationDto);
            if (specialization == null)
            {
                return NotFound(new { message = "Specialization not found" });
            }

            return Ok(specialization);
        }

        /// <summary>
        /// Delete specialization (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _specializationService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Specialization not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
