using System.Threading.Tasks;
using MediCare.API.Extensions;
using MediCare.Services.Dtos;
using MediCare.Services.Enums;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.API.Controllers
{
    // The notifications of the logged in user
    [Route("api/v1/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;

        public NotificationsController(INotificationService notificationService, IWebHostEnvironment environment)
        {
            _notificationService = notificationService;
            _environment = environment;
        }

        // GET api/v1/notifications/me?unreadOnly=false&page=1&pageSize=20
        [HttpGet("me")]
        [ProducesResponseType(typeof(PagedResultDto<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            PagedResultDto<NotificationDto> result =
                await _notificationService.GetMyNotificationsAsync(User.GetUserId(), unreadOnly, page, pageSize);
            return Ok(result);
        }

        // GET api/v1/notifications/unread-count   (for the bell icon)
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUnreadCount()
        {
            UnreadCountDto dto = new UnreadCountDto();
            dto.Count = await _notificationService.GetUnreadCountAsync(User.GetUserId());
            return Ok(dto);
        }

        // PATCH api/v1/notifications/5/read
        [HttpPatch("{id:int}/read")]
        [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            NotificationDto dto = await _notificationService.MarkAsReadAsync(User.GetUserId(), id);
            return Ok(dto);
        }

        // POST api/v1/notifications/read-all
        [HttpPost("read-all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(User.GetUserId());
            return NoContent();
        }

        // POST api/v1/notifications/dev-test
        // DEVELOPMENT ONLY: create a test notification for the logged in user. Returns 404 in any other environment
        // Removed when the appointment notifications exist
        [HttpPost("dev-test")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateTestNotification()
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            await _notificationService.CreateAsync(
                User.GetUserId(),
                NotificationType.AppointmentReminder,
                "Test notification",
                "This is a test notification created by the development endpoint.");

            return NoContent();
        }
    }
}