using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediCare.Web.Models;
using MediCare.Web.Models.Api;
using MediCare.Web.Services;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// Login, register, logout. The rules (password, duplicate email, ...) are checked by the API, not here.
public class AccountController : Controller
{
    private readonly IMediCareApiClient _api;

    public AccountController(IMediCareApiClient api)
    {
        _api = api;
    }

    // ---------- Login ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, bool expired = false)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        LoginViewModel model = new LoginViewModel();
        model.ReturnUrl = returnUrl;
        model.SessionExpired = expired;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            LoginRequestDto request = new LoginRequestDto();
            request.Email = model.Email;
            request.Password = model.Password;

            LoginResponseDto result = await _api.LoginAsync(request);
            await SignInAsync(result.User, result.Token, result.ExpiresAtUtc);

            // Go back to the page the user wanted, but only if it is a page of this site.
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (ApiException ex)
        {
            // 401 wrong data, 403 inactive account, 423 locked, 429 too many requests: the message comes from the API.
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // ---------- Register patient ----------

    [HttpGet]
    public IActionResult RegisterPatient()
    {
        return View(new RegisterPatientViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> RegisterPatient(RegisterPatientViewModel model)
    {
        if (model.DateOfBirth != null && model.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            ModelState.AddModelError(nameof(model.DateOfBirth), "The date of birth cannot be in the future.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            RegisterPatientRequestDto request = new RegisterPatientRequestDto();
            request.FullName = model.FullName;
            request.Email = model.Email;
            request.PhoneNumber = model.PhoneNumber;
            request.Password = model.Password;
            request.DateOfBirth = model.DateOfBirth!.Value;
            request.Gender = model.Gender;

            // The API logs the new user in at once (it returns a token), so we sign in directly.
            LoginResponseDto result = await _api.RegisterPatientAsync(request);
            await SignInAsync(result.User, result.Token, result.ExpiresAtUtc);

            return RedirectToAction("Index", "Home");
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(model);
        }
    }

    // ---------- Register doctor ----------

    [HttpGet]
    public IActionResult RegisterDoctor()
    {
        return View(new RegisterDoctorViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> RegisterDoctor(RegisterDoctorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            RegisterDoctorRequestDto request = new RegisterDoctorRequestDto();
            request.FullName = model.FullName;
            request.Email = model.Email;
            request.PhoneNumber = model.PhoneNumber;
            request.Password = model.Password;
            request.SpecializationId = model.SpecializationId;
            request.LicenseNumber = model.LicenseNumber;
            request.ConsultationFee = model.ConsultationFee;

            // The new doctor is Pending: Home sends them to the PendingApproval page.
            LoginResponseDto result = await _api.RegisterDoctorAsync(request);
            await SignInAsync(result.User, result.Token, result.ExpiresAtUtc);

            return RedirectToAction("Index", "Home");
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(model);
        }
    }

    // ---------- Pending doctor ----------

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> PendingApproval()
    {
        string role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        if (role != AppRoles.Doctor)
        {
            return RedirectToAction("Index", "Home");
        }

        // The cookie can be old, so we ask the API for the current status.
        UserDto me = await _api.GetMeAsync();

        if (me.DoctorApprovalStatus == DoctorApprovalStatuses.Approved)
        {
            // The Admin approved the doctor: renew the cookie with the new status, keeping the same token and expiry.
            string? token = await HttpContext.GetTokenAsync("access_token");
            AuthenticateResult authResult =
                await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            DateTimeOffset? expires = authResult.Properties?.ExpiresUtc;

            if (token != null && expires != null)
            {
                await SignInAsync(me, token, expires.Value.UtcDateTime);
                return RedirectToAction("Index", "Home");
            }
        }

        return View(me);
    }

    // ---------- Logout and access denied ----------

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Removes the session (and the API token) from the server memory and deletes the cookie.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ---------- Helpers ----------

    // Creates the login cookie. The API token is stored inside the session on the SERVER, never in the browser.
    private async Task SignInAsync(UserDto user, string token, DateTime expiresAtUtc)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        if (user.PatientId != null)
        {
            claims.Add(new Claim(AppClaims.PatientId, user.PatientId.Value.ToString()));
        }

        if (user.DoctorId != null)
        {
            claims.Add(new Claim(AppClaims.DoctorId, user.DoctorId.Value.ToString()));
        }

        if (user.DoctorApprovalStatus != null)
        {
            claims.Add(new Claim(AppClaims.DoctorApprovalStatus, user.DoctorApprovalStatus));
        }

        ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

        AuthenticationProperties properties = new AuthenticationProperties();
        properties.ExpiresUtc = expiresAtUtc;     // never longer than the API token
        properties.IsPersistent = false;          // the cookie is removed when the browser closes
        properties.AllowRefresh = false;
        properties.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = token } });

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    // Shows the API's error messages on the form.
    private void AddApiErrors(ApiException ex)
    {
        // 409 = the email is already used.
        if (ex.StatusCode == 409)
        {
            ModelState.AddModelError("Email", ex.Message);
            return;
        }

        // 400 with a list of field errors.
        if (ex.Errors != null && ex.Errors.Count > 0)
        {
            foreach (KeyValuePair<string, string[]> item in ex.Errors)
            {
                foreach (string message in item.Value)
                {
                    ModelState.AddModelError(string.Empty, message);
                }
            }

            return;
        }

        ModelState.AddModelError(string.Empty, ex.Message);
    }
}