using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Security
{
    public static class RoleNames
    {
        public const string Patient = "Patient";
        public const string Doctor = "Doctor";
        public const string Admin = "Admin";

        public static readonly string[] All = { Patient, Doctor, Admin };
    }
}
