using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<UserDto>>> GetAll([FromQuery] PaginationParams paginationParams)
        {
            var users = await _userService.GetAllAsync(paginationParams);
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email already exists
            if (await _userService.ExistsAsync(createUserDto.Email))
            {
                return BadRequest(new { message = "Email already registered" });
            }

            var user = await _userService.CreateAsync(createUserDto);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.UpdateAsync(id, updateUserDto);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Partial update user (PATCH)
        /// </summary>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> Patch(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            return await Update(id, updateUserDto);
        }

        /// <summary>
        /// Delete/deactivate user
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return NoContent();
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        [HttpGet("exists/{email}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> Exists(string email)
        {
            var exists = await _userService.ExistsAsync(email);
            return Ok(exists);
        }
    }
}
