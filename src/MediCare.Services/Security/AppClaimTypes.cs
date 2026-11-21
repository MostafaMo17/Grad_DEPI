using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Security
{
    public static class AppClaimTypes
    {
    
        // Added by the API on every request (read from the database, not from the token)
        public const string DoctorApprovalStatus = "DoctorApprovalStatus";
    }
}
