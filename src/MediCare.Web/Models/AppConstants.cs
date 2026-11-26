namespace MediCare.Web.Models;

// The role names used by the API.
public static class AppRoles
{
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Admin = "Admin";
}

// Names of the claims we put in the login cookie.
public static class AppClaims
{
    public const string PatientId = "PatientId";
    public const string DoctorId = "DoctorId";
    public const string DoctorApprovalStatus = "DoctorApprovalStatus";
}

public static class DoctorApprovalStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}