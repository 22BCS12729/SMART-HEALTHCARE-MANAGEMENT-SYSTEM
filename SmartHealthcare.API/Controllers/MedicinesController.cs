using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Doctor")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;
        private readonly ILogger<MedicinesController> _logger;

        public MedicinesController(IMedicineService medicineService, ILogger<MedicinesController> logger)
        {
            _medicineService = medicineService;
            _logger = logger;
        }

        /// <summary>
        /// Get all medicines with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<MedicineDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<MedicineDto>>> GetAll([FromQuery] PaginationParams paginationParams)
        {
            var medicines = await _medicineService.GetAllAsync(paginationParams);
            return Ok(medicines);
        }

        /// <summary>
        /// Search medicines with filters
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(PagedResultDto<MedicineListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<MedicineListDto>>> Search(
            [FromQuery] MedicineSearchDto searchDto,
            [FromQuery] PaginationParams paginationParams)
        {
            var medicines = await _medicineService.SearchAsync(searchDto, paginationParams);
            return Ok(medicines);
        }

        /// <summary>
        /// Get medicine by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicineDto>> GetById(int id)
        {
            var medicine = await _medicineService.GetByIdAsync(id);
            if (medicine == null)
            {
                return NotFound(new { message = "Medicine not found" });
            }

            return Ok(medicine);
        }

        /// <summary>
        /// Create new medicine (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MedicineDto>> Create([FromBody] CreateMedicineDto createMedicineDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var medicine = await _medicineService.CreateAsync(createMedicineDto);
            return CreatedAtAction(nameof(GetById), new { id = medicine.Id }, medicine);
        }

        /// <summary>
        /// Update medicine (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MedicineDto>> Update(int id, [FromBody] UpdateMedicineDto updateMedicineDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var medicine = await _medicineService.UpdateAsync(id, updateMedicineDto);
            if (medicine == null)
            {
                return NotFound(new { message = "Medicine not found" });
            }

            return Ok(medicine);
        }

        /// <summary>
        /// Partial update medicine (Admin only, PATCH)
        /// </summary>
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicineDto>> Patch(int id, [FromBody] UpdateMedicineDto updateMedicineDto)
        {
            return await Update(id, updateMedicineDto);
        }

        /// <summary>
        /// Delete medicine (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _medicineService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Medicine not found" });
            }

            return NoContent();
        }
    }
}
