using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Security
{
    // Added by the API on every request (read from the database, not from the token)
    public static class AppClaimTypes
    {
    
        public const string DoctorApprovalStatus = "DoctorApprovalStatus";

        public const string PatientId = "PatientId";
        public const string DoctorId = "DoctorId";
    }
}
