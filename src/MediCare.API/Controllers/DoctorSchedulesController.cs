using MediCare.API.Extensions;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/v1/doctor/schedules")]
    [Authorize(Policy = "ApprovedDoctor")]
    public class DoctorSchedulesController : ControllerBase
    {
        private readonly IDoctorScheduleService _scheduleService;

        public DoctorSchedulesController(IDoctorScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMySchedules()
        {
            var schedules = await _scheduleService
                .GetMySchedulesAsync(User.GetUserId());

            return Ok(schedules);
        }
    }
}