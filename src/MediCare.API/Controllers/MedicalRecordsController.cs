
using MediCare.Services.Dtos;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCare.API.Controllers
{
    [Route("api/v1/medical-records")]
    [ApiController]
    [Authorize(Roles = RoleNames.Doctor + "," + RoleNames.Patient)]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordsController(
            IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        // GET: api/v1/medical-records/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            int userId = GetCurrentUserId();
            string role = GetCurrentUserRole();

            var result = await _medicalRecordService.GetByIdAsync(
                id, userId, role);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // GET: api/v1/medical-records/appointments/{appointmentId}
        [HttpGet("appointments/{appointmentId:int}")]
        public async Task<IActionResult> GetByAppointmentId(int appointmentId)
        {
            int userId = GetCurrentUserId();
            string role = GetCurrentUserRole();

            var result = await _medicalRecordService.GetByAppointmentIdAsync(
                appointmentId, userId, role);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // GET: api/v1/medical-records/my-records
        [HttpGet("my-records")]
        public async Task<IActionResult> GetMyRecords()
        {
            int userId = GetCurrentUserId();
            string role = GetCurrentUserRole();

            var result = await _medicalRecordService.GetMyRecordsAsync(
                userId, role);

            return Ok(result);
        }

        // POST: api/v1/medical-records/appointments/{appointmentId}
        [HttpPost("appointments/{appointmentId:int}")]
        [Authorize(Roles = RoleNames.Doctor, Policy = "ApprovedDoctor")]
        public async Task<IActionResult> Create(
            int appointmentId,
            [FromBody] CreateMedicalRecordDto dto)
        {
            int doctorUserId = GetCurrentUserId();

            var result = await _medicalRecordService.CreateAsync(
                appointmentId,
                doctorUserId,
                dto);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        private int GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int parsedUserId))
                throw new UnauthorizedAccessException("Invalid user identity.");

            return parsedUserId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("User role not found.");
        }
    }
}
