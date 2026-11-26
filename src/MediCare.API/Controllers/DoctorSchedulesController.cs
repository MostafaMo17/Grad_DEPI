using MediCare.API.Extensions;
using MediCare.Services.Dtos;
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

        // GET api/v1/doctor/schedules
        [HttpGet]
        public async Task<IActionResult> GetMySchedules()
        {
            var schedules = await _scheduleService
                .GetMySchedulesAsync(User.GetUserId());

            return Ok(schedules);
        }

        // PUT api/v1/doctor/schedules
        // Replaces the whole weekly schedule. Days left out become days off.
        [HttpPut]
        public async Task<IActionResult> SaveMySchedules([FromBody] SaveDoctorScheduleDto dto)
        {
            var schedules = await _scheduleService
                .SaveMySchedulesAsync(User.GetUserId(), dto);

            return Ok(schedules);
        }
    }
}
