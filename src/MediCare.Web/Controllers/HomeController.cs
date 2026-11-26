using System.Diagnostics;
using System.Security.Claims;
using MediCare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class HomeController : Controller
{
    // The public home page. A logged-in user goes straight to the dashboard.
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard");
        }

        return View();
    }

    // Placeholder dashboard. The real dashboards (patient, doctor, admin) are built by the other modules.
    [Authorize]
    public IActionResult Dashboard()
    {
        string role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        string status = User.FindFirst(AppClaims.DoctorApprovalStatus)?.Value ?? string.Empty;

        // A doctor that is not approved yet can only see the "pending approval" page.
        if (role == AppRoles.Doctor && status != DoctorApprovalStatuses.Approved)
        {
            return RedirectToAction("PendingApproval", "Account");
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}