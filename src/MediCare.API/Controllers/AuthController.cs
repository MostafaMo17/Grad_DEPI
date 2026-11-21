using MediCare.API.Extensions;
using MediCare.Services.Dtos;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MediCare.API.Controllers
{
    // Register, login 
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
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientDto dto)
        {
            LoginResponseDto response = await _authService.RegisterPatientAsync(dto);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        // POST api/v1/auth/register/doctor
        [HttpPost("register/doctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDto dto)
        {
            LoginResponseDto response = await _authService.RegisterDoctorAsync(dto);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        // POST api/v1/auth/login
        // Limited to 10 requests per minute per IP address 
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            LoginResponseDto response = await _authService.LoginAsync(dto);
            return Ok(response);
        }

        // GET api/v1/auth/me
        // The claims were filled from the database by JwtTokenEvents on this same request
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            UserDto dto = new UserDto();
            dto.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            dto.FullName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            dto.Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            dto.Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            dto.DoctorApprovalStatus = User.FindFirst(AppClaimTypes.DoctorApprovalStatus)?.Value;

            return Ok(dto);
        }
    }
}