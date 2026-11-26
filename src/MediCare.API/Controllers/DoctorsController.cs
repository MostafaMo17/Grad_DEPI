
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.API.Controllers
{
    [Route("api/v1/doctors")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetApprovedDoctors()
        {
            var doctors = await _doctorService.GetApprovedDoctorsAsync();
            return Ok(doctors);
        }

        [HttpGet("{doctorId:int}")]
        public async Task<IActionResult> GetDoctorById(int doctorId)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);

            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{doctorId:int}/approve")]
        public async Task<IActionResult> ApproveDoctor(int doctorId)
        {
            await _doctorService.ApproveDoctorAsync(doctorId);

            return Ok(new
            {
                message = "Doctor approved successfully."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{doctorId:int}/reject")]
        public async Task<IActionResult> RejectDoctor(int doctorId)
        {
            await _doctorService.RejectDoctorAsync(doctorId);

            return Ok(new
            {
                message = "Doctor rejected successfully."
            });
        }
    }
}
