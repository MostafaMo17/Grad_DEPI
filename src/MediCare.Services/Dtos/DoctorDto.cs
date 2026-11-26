namespace MediCare.Services.Dtos
{
    public class DoctorDto
    {
        public int DoctorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        public decimal ConsultationFee { get; set; }

        public string? ProfileImage { get; set; }
    }
}