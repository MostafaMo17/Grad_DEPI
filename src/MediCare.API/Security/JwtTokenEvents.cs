using MediCare.Services.Dtos;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace MediCare.API.Security
{
    // Runs after ASP.NET has checked the token signature, issuer, audience and expiry
    public static class JwtTokenEvents
    {
        public static async Task OnTokenValidated(TokenValidatedContext context)
        {
            ClaimsIdentity? identity = context.Principal?.Identity as ClaimsIdentity;
            string? userIdText = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (identity == null || !int.TryParse(userIdText, out int userId))
            {
                context.Fail("The token is not valid.");
                return;
            }

            IAuthService authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            UserDto? user = await authService.GetUserForTokenAsync(userId);

            if (user == null)
            {
                // The user is deactivated or deleted, the token is no longer accepted (401).
                context.Fail("The token is no longer valid.");
                return;
            }

            // Add fresh claims read from the database.
            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));

            if (user.DoctorApprovalStatus != null)
            {
                identity.AddClaim(new Claim(AppClaimTypes.DoctorApprovalStatus, user.DoctorApprovalStatus));
            }
        }
    }
}
