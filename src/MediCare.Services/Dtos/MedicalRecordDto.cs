
namespace MediCare.Services.Dtos
{
    public class MedicalRecordDto
    {
        public int MedicalRecordId { get; set; }

        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string Symptoms { get; set; } = string.Empty;

        public string Diagnosis { get; set; } = string.Empty;

        public string TreatmentNotes { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
    }
}
