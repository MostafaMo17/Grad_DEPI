using MediCare.Web.Models;
using MediCare.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// The doctor's weekly clinic hours (view + edit), calling MediCare.API.
[Authorize(Roles = "Doctor")]
public class DoctorScheduleController : Controller
{
    private static readonly DayOfWeek[] WeekOrder =
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    };

    private readonly MediCareApiClient _apiClient;

    public DoctorScheduleController(MediCareApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private string Token => User.FindFirst("access_token")?.Value ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var schedules = await LoadSchedulesAsync();
        if (schedules == null)
        {
            return RedirectToLoginWithExpiry();
        }

        var model = new WeeklyScheduleViewModel();

        foreach (var day in WeekOrder)
        {
            var entry = schedules.FirstOrDefault(s => s.DayOfWeek == day);

            model.Days.Add(new ScheduleDayRowViewModel
            {
                DayName = day.ToString(),
                IsWorking = entry != null && entry.IsAvailable,
                HoursText = entry != null
                    ? $"{entry.StartTime:HH\\:mm} - {entry.EndTime:HH\\:mm}"
                    : "Off-Duty",
                SlotLengthMinutes = entry?.SlotLengthMinutes ?? 0,
                IsToday = day == DateTime.Today.DayOfWeek
            });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var schedules = await LoadSchedulesAsync();
        if (schedules == null)
        {
            return RedirectToLoginWithExpiry();
        }

        var model = new EditScheduleViewModel();

        // All days were saved with one shared range, so the first row refills the form.
        var first = schedules.OrderBy(s => s.DayOfWeek).FirstOrDefault();
        if (first != null)
        {
            model.StartTime = first.StartTime;
            model.EndTime = first.EndTime;
            model.SlotLengthMinutes = first.SlotLengthMinutes;
        }

        foreach (var day in WeekOrder)
        {
            model.Days.Add(new DayToggleViewModel
            {
                DayOfWeek = day,
                DayName = day.ToString(),
                IsWorking = schedules.Any(s => s.DayOfWeek == day)
            });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditScheduleViewModel model)
    {
        if (model.EndTime <= model.StartTime)
        {
            ModelState.AddModelError(nameof(model.EndTime),
                "The end time must be after the start time.");
        }

        if (!model.Days.Any(d => d.IsWorking))
        {
            ModelState.AddModelError(string.Empty,
                "Pick at least one working day, or all days stay off.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var days = model.Days
            .Where(d => d.IsWorking)
            .Select(d => new ApiScheduleDay
            {
                DayOfWeek = d.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                SlotLengthMinutes = model.SlotLengthMinutes
            })
            .ToList();

        try
        {
            await _apiClient.SaveMySchedulesAsync(Token, days);
        }
        catch (ApiException ex) when (ex.StatusCode is 401 or 403)
        {
            return RedirectToLoginWithExpiry();
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty,
                "Cannot reach the API. Make sure MediCare.API is running.");
            return View(model);
        }

        TempData["Success"] = "Your weekly clinic hours were saved.";
        return RedirectToAction(nameof(Index));
    }

    // Returns the schedule list, or null when the API says the login expired.
    private async Task<List<ApiDoctorSchedule>?> LoadSchedulesAsync()
    {
        try
        {
            return await _apiClient.GetMySchedulesAsync(Token);
        }
        catch (ApiException ex) when (ex.StatusCode is 401 or 403)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Cannot reach the API. Make sure MediCare.API is running.";
            return new List<ApiDoctorSchedule>();
        }
    }

    private IActionResult RedirectToLoginWithExpiry()
    {
        TempData["Error"] = "Please sign in again.";
        return RedirectToAction("Login", "Account");
    }
}