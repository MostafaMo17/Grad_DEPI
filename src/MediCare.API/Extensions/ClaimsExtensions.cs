using MediCare.Services.Security;
using System.Security.Claims;

namespace MediCare.API.Extensions
{
    // Extension methods on User (the ClaimsPrincipal) inside any controller.
    // Instead of User.FindFirst(ClaimTypes.NameIdentifier)?.Value every time, we write User.GetUserId().
    // All values come from the claims that JwtTokenEvents filled from the database on this request.

    public static class ClaimsExtensions
    {
        // The id of the logged in user
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

        // Patient, Doctor , Admin
        public static string GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // The patient profile id , null if he is not
        public static int? GetPatientId(this ClaimsPrincipal user)
        {
            string? value = user.FindFirst(AppClaimTypes.PatientId)?.Value;
            if (int.TryParse(value, out int patientId))
            {
                return patientId;
            }

            return null;
        }

        // The doctor profile id, null if he is not
        public static int? GetDoctorId(this ClaimsPrincipal user)
        {
            string? value = user.FindFirst(AppClaimTypes.DoctorId)?.Value;
            if (int.TryParse(value, out int doctorId))
            {
                return doctorId;
            }

            return null;
        }

        public static string? GetDoctorApprovalStatus(this ClaimsPrincipal user)
        {
            return user.FindFirst(AppClaimTypes.DoctorApprovalStatus)?.Value;
        }
    }
}