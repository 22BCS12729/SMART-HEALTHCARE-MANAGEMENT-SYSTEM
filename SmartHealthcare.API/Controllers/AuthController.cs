using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Enums;
using SmartHealthcare.Core.Interfaces;

namespace SmartHealthcare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IUserService userService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Register a new patient
        /// </summary>
        [HttpPost("register/patient")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PatientDto>> RegisterPatient([FromBody] CreatePatientDto createPatientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email already exists
            if (await _userService.ExistsAsync(createPatientDto.Email))
            {
                return BadRequest(new { message = "Email already registered" });
            }

            var patientService = HttpContext.RequestServices.GetRequiredService<IPatientService>();
            var patient = await patientService.CreateAsync(createPatientDto);

            return CreatedAtAction(nameof(PatientsController.GetById), "Patients", new { id = patient.Id }, patient);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Revoke refresh token (logout)
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken);

            if (!result)
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            return Ok(new { message = "Token revoked successfully" });
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

            if (!result)
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
    }
}
