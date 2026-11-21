namespace MediCare.Services.Enums
{
    public enum NotificationType
    {
        AppointmentBooked = 0,
        AppointmentConfirmed = 1,
        AppointmentRejected = 2,     
        AppointmentCancelled = 3,     
        AppointmentRescheduled = 4,   
        AppointmentReminder = 5,
        DoctorApproved = 6,
        DoctorRejected = 7,
        PrescriptionIssued = 8
    }
}