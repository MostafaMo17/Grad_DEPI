using MediCare.Services.Dtos;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCare.API.Security
{
    // Runs after ASP.NET has checked the token signature, issuer, audience and expiry
    public static class JwtTokenEvents
    {
        //  SignalR sends the token in the query string ,a query-string token ONLY for the hub path
        public static Task OnMessageReceived(MessageReceivedContext context)
        {
            string? accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken) && context.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }

        public static async Task OnTokenValidated(TokenValidatedContext context)
        {
            ClaimsIdentity? identity = context.Principal?.Identity as ClaimsIdentity;
            string? userIdText = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (identity == null || !int.TryParse(userIdText, out int userId))
            {
                context.Fail("The token is not valid.");
                return;
            }

            // A hub token (scope = hub) is accepted only on the hub path, It must not work on normal endpoints
            string? scope = context.Principal?.FindFirst(AppClaimTypes.Scope)?.Value;
            bool isHubPath = context.HttpContext.Request.Path.StartsWithSegments("/hubs");
            if (scope == AppClaimTypes.HubScope && !isHubPath)
            {
                context.Fail("This token can only be used for the notification hub.");
                return;
            }

            IAuthService authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            UserDto? user = await authService.GetUserForTokenAsync(userId);

            if (user == null)
            {
                // The user is deactivated or deleted: the token is no longer accepted (401).
                context.Fail("The token is no longer valid.");
                return;
            }

            // Add fresh claims read from the database
            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));

            if (user.PatientId != null)
            {
                identity.AddClaim(new Claim(AppClaimTypes.PatientId, user.PatientId.Value.ToString()));
            }

            if (user.DoctorId != null)
            {
                identity.AddClaim(new Claim(AppClaimTypes.DoctorId, user.DoctorId.Value.ToString()));
            }

            if (user.DoctorApprovalStatus != null)
            {
                identity.AddClaim(new Claim(AppClaimTypes.DoctorApprovalStatus, user.DoctorApprovalStatus));
            }
        }

        // no token, or an invalid token: return 401 with a JSON body 
        public static async Task OnChallenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();

            ProblemDetails problem = new ProblemDetails();
            problem.Status = 401;
            problem.Title = "Unauthorized";
            problem.Detail = "You must log in with a valid token to use this endpoint.";
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Response.StatusCode = 401;
            context.Response.Headers["WWW-Authenticate"] = "Bearer";
            await context.Response.WriteAsJsonAsync(problem);
        }

        // A valid token, but the user is not allowed : return 403 with a JSON body
        public static async Task OnForbidden(ForbiddenContext context)
        {
            ProblemDetails problem = new ProblemDetails();
            problem.Status = 403;
            problem.Title = "Forbidden";
            problem.Detail = "You do not have permission to use this endpoint.";
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
