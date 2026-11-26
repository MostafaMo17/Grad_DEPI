
using MediCare.Services.Dtos;
using MediCare.Services.Entities;
using MediCare.Services.Enums;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using MediCare.Services.Exceptions;

namespace MediCare.Services.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MedicalRecordService(
            IMedicalRecordRepository medicalRecordRepository,
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository,
            IUnitOfWork unitOfWork)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MedicalRecordDto?> GetByIdAsync(
            int medicalRecordId,
            int currentUserId,
            string currentRole)
        {
            var record = await _medicalRecordRepository
                .GetByIdAsync(medicalRecordId);

            if (record == null)
                return null;

            EnsureCanAccess(record, currentUserId, currentRole);

            return MapToDto(record);
        }

        public async Task<MedicalRecordDto?> GetByAppointmentIdAsync(
            int appointmentId,
            int currentUserId,
            string currentRole)
        {
            var record = await _medicalRecordRepository
                .GetByAppointmentIdAsync(appointmentId);

            if (record == null)
                return null;

            EnsureCanAccess(record, currentUserId, currentRole);

            return MapToDto(record);
        }

        public async Task<IReadOnlyList<MedicalRecordDto>> GetMyRecordsAsync(
            int currentUserId,
            string currentRole)
        {
            IReadOnlyList<MedicalRecord> records;

            if (currentRole == RoleNames.Patient)
            {
                records = await _medicalRecordRepository
                    .GetByPatientUserIdAsync(currentUserId);
            }
            else if (currentRole == RoleNames.Doctor)
            {
                records = await _medicalRecordRepository
                    .GetByDoctorUserIdAsync(currentUserId);
            }
            else
            {
                throw new ForbiddenException(
                    "You are not authorized to access medical records.");
            }

            return records.Select(MapToDto).ToList();
        }

        public async Task<MedicalRecordDto> CreateAsync(
            int appointmentId,
            int doctorUserId,
            CreateMedicalRecordDto dto)
        {
            var doctor = await _doctorRepository
                .GetByUserIdAsync(doctorUserId);

            if (doctor == null)
                throw new NotFoundException("Doctor profile not found.");

            var appointment = await _appointmentRepository
                .GetByIdAsync(appointmentId);

            if (appointment == null)
                throw new NotFoundException("Appointment not found.");

            if (appointment.DoctorId != doctor.DoctorId)
                throw new ForbiddenException(
                    "You are not authorized to create a record for this appointment.");

            if (appointment.Status != AppointmentStatus.Confirmed &&
                appointment.Status != AppointmentStatus.Completed)
            {
                throw new BadRequestException(
                    "Medical records can only be created for confirmed or completed appointments.");
            }

            var existingRecord = await _medicalRecordRepository
                .GetByAppointmentIdAsync(appointmentId);

            if (existingRecord != null)
                throw new ConflictException(
                    "A medical record already exists for this appointment.");

            var record = new MedicalRecord
            {
                AppointmentId = appointmentId,
                Symptoms = dto.Symptoms,
                Diagnosis = dto.Diagnosis,
                TreatmentNotes = dto.TreatmentNotes,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _medicalRecordRepository.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            return new MedicalRecordDto
            {
                MedicalRecordId = record.MedicalRecordId,
                AppointmentId = record.AppointmentId,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient.User.FullName,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.User.FullName,
                Symptoms = record.Symptoms,
                Diagnosis = record.Diagnosis,
                TreatmentNotes = record.TreatmentNotes,
                CreatedAtUtc = record.CreatedAtUtc
            };
        }

        private static void EnsureCanAccess(
            MedicalRecord record,
            int currentUserId,
            string currentRole)
        {
            bool isAuthorized =
                (currentRole == RoleNames.Doctor &&
                 record.Appointment.Doctor.UserId == currentUserId)
                ||
                (currentRole == RoleNames.Patient &&
                 record.Appointment.Patient.UserId == currentUserId);

            if (!isAuthorized)
            {
                throw new ForbiddenException(
                    "You are not authorized to access this medical record.");
            }
        }

        private static MedicalRecordDto MapToDto(MedicalRecord record)
        {
            return new MedicalRecordDto
            {
                MedicalRecordId = record.MedicalRecordId,
                AppointmentId = record.AppointmentId,
                PatientId = record.Appointment.PatientId,
                PatientName = record.Appointment.Patient.User.FullName,
                DoctorId = record.Appointment.DoctorId,
                DoctorName = record.Appointment.Doctor.User.FullName,
                Symptoms = record.Symptoms,
                Diagnosis = record.Diagnosis,
                TreatmentNotes = record.TreatmentNotes,
                CreatedAtUtc = record.CreatedAtUtc
            };
        }
    }
}
