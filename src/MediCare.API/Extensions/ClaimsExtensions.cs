using MediCare.Services.Security;
using System.Security.Claims;

namespace MediCare.API.Extensions
{
    // Extension methods on User (the ClaimsPrincipal) inside any controller.
    // Instead of User.FindFirst(ClaimTypes.NameIdentifier)?.Value every time, we write User.GetUserId().
    public static class ClaimsExtensions
    {
        // The id of the logged-in user. Returns 0 if there is none.
        public static int GetUserId(this ClaimsPrincipal user)
        {
            string? value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(value, out int userId))
            {
                return userId;
            }

            return 0;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public static string? GetDoctorApprovalStatus(this ClaimsPrincipal user)
        {
            return user.FindFirst(AppClaimTypes.DoctorApprovalStatus)?.Value;
        }
    }
}
