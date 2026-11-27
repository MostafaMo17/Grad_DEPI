using System.Threading.Tasks;
using MediCare.Web.Models.Api;
using MediCare.Web.Services;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// The notifications of the logged in user
// All the data comes from the API, The browser never receives the API's JWT
[Authorize]
public class NotificationsController : Controller
{
    private const int PageSize = 20;

    private readonly IMediCareApiClient _api;
    private readonly IWebHostEnvironment _environment;

    public NotificationsController(IMediCareApiClient api, IWebHostEnvironment environment)
    {
        _api = api;
        _environment = environment;
    }

    // Pages 

    [HttpGet]
    public async Task<IActionResult> Index(bool unreadOnly = false, int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        PagedResultDto<NotificationDto> result = await _api.GetNotificationsAsync(unreadOnly, page, PageSize);

        NotificationsViewModel model = new NotificationsViewModel();
        model.Items = result.Items;
        model.Page = result.Page;
        model.PageSize = result.PageSize;
        model.TotalCount = result.TotalCount;
        model.UnreadOnly = unreadOnly;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(int id, bool unreadOnly = false, int page = 1)
    {
        try
        {
            await _api.MarkNotificationAsReadAsync(id);
        }
        catch (ApiException ex)
        {
            // the notification does not exist or is not yours
            if (ex.StatusCode != 404)
            {
                throw;
            }
        }

        return RedirectToAction(nameof(Index), new { unreadOnly, page });
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        await _api.MarkAllNotificationsAsReadAsync();
        return RedirectToAction(nameof(Index));
    }

    // TEMPORARY (development only): creates a test notification
    // Deleted with the API's dev-test endpoint
    [HttpPost]
    public async Task<IActionResult> CreateTest()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        await _api.CreateTestNotificationAsync();
        return RedirectToAction(nameof(Index));
    }

    //  JSON for notifications.js (the bell and the live connection) 

    [HttpGet]
    public async Task<IActionResult> UnreadCount()
    {
        int count = await _api.GetUnreadCountAsync();
        return Json(new { count });
    }

    // Gives the browser the SHORT token (5 minutes) for the real time hub
    // The user's normal API token never leaves the server: this method calls the API with it and returns only the short one
    [HttpGet]
    public async Task<IActionResult> HubToken()
    {
        Response.Headers["Cache-Control"] = "no-store";

        HubTokenDto result = await _api.GetHubTokenAsync();
        return Json(new { token = result.Token });
    }
}