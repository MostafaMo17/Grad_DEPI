using System.Threading.Tasks;
using MediCare.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MediCare.Web.Filters;

// Runs when a controller action throws, no controller repeats it
public class ApiExceptionFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        ApiException? apiException = context.Exception as ApiException;
        if (apiException == null)
        {
            return;
        }

        // JavaScript requests (fetch) send this header. They need a status code, not a redirect.
        bool isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (apiException.StatusCode == 401)
        {
            // The API token expired or the account was deactivated: end the session.
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (isAjax)
            {
                context.Result = new StatusCodeResult(401);
            }
            else
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { expired = true });
            }

            context.ExceptionHandled = true;
        }
        else if (apiException.StatusCode == 403)
        {
            if (isAjax)
            {
                context.Result = new StatusCodeResult(403);
            }
            else
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }

            context.ExceptionHandled = true;
        }
    }
}