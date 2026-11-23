using System.Threading.Tasks;
using MediCare.API.Extensions;
using MediCare.Services.Dtos;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MediCare.API.Controllers
{
    // Register, Login , ME

    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/v1/auth/register/patient
        [HttpPost("register/patient")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientDto dto)
        {
            LoginResponseDto response = await _authService.RegisterPatientAsync(dto);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        // POST api/v1/auth/register/doctor
        [HttpPost("register/doctor")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDto dto)
        {
            LoginResponseDto response = await _authService.RegisterDoctorAsync(dto);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        // POST api/v1/auth/login
        // Limited to 10 requests per minute per IP address 
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            LoginResponseDto response = await _authService.LoginAsync(dto);
            return Ok(response);
        }

        // GET api/v1/auth/me
        // The claims were filled from the database by JwtTokenEvents on this same request
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public IActionResult Me()
        {
            UserDto dto = new UserDto();
            dto.UserId = User.GetUserId();
            dto.FullName = User.GetFullName();
            dto.Email = User.GetEmail();
            dto.Role = User.GetRole();
            dto.PatientId = User.GetPatientId();
            dto.DoctorId = User.GetDoctorId();
            dto.DoctorApprovalStatus = User.GetDoctorApprovalStatus();

            return Ok(dto);
        }

        // GET api/v1/auth/doctor-check
        // TEMPORARY test endpoint for the "ApprovedDoctor" policy
        // Removed when the doctor endpoints exist
        [HttpGet("doctor-check")]
        [Authorize(Policy = "ApprovedDoctor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public IActionResult DoctorCheck()
        {
            return Ok(new { message = "You are an approved doctor." });
        }
    }
}