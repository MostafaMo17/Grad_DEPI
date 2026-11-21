using MediCare.Services.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MediCare.Services.Enums;

namespace MediCare.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Specialization> Specializations => Set<Specialization>();
        public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
        public DbSet<DoctorVacation> DoctorVacations => Set<DoctorVacation>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region // ---------- Accounts (Identity) ----------

            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.Email).IsRequired();
                b.Property(u => u.NormalizedEmail).IsRequired();
                b.HasIndex(u => u.NormalizedEmail).IsUnique();
            });

           
            builder.Entity<IdentityUserRole<int>>()
                .HasIndex(ur => ur.UserId)
                .IsUnique();
            #endregion

            #region // ---------- Profiles ----------

            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Doctor>()
                .HasOne(d => d.Specialization)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecializationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Specialization>()
                .HasIndex(s => s.Name)
                .IsUnique();


            builder.Entity<Patient>(b =>
            {
                b.Property(p => p.Gender)
                    .HasConversion<string>()
                    .HasMaxLength(10);

                b.ToTable(t => t.HasCheckConstraint(
                    "CK_Patients_Gender",
                    "[Gender] IN ('Male', 'Female')"));
            });

            builder.Entity<Doctor>(b =>
            {
                b.Property(d => d.ApprovalStatus)
                    .HasConversion<string>()
                    .HasMaxLength(16)
                    .HasDefaultValue(DoctorApprovalStatus.Pending);

                b.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Doctors_ApprovalStatus",
                        "[ApprovalStatus] IN ('Pending', 'Approved', 'Rejected')");

                    t.HasCheckConstraint(
                        "CK_Doctors_ConsultationFee",
                        "[ConsultationFee] >= 0");
                });
            });
            #endregion

            #region // ---------- Doctor schedules and vacations ----------

            builder.Entity<DoctorSchedule>(b =>
            {
                b.HasOne(s => s.Doctor)
                    .WithMany(d => d.DoctorSchedules)
                    .HasForeignKey(s => s.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_DoctorSchedules_TimeRange", "[EndTime] > [StartTime]");
                    t.HasCheckConstraint("CK_DoctorSchedules_SlotLength", "[SlotLengthMinutes] > 0");
                });
            });

            builder.Entity<DoctorVacation>(b =>
            {
                b.HasOne(v => v.Doctor)
                    .WithMany(d => d.DoctorVacations)
                    .HasForeignKey(v => v.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.ToTable(t => t.HasCheckConstraint("CK_DoctorVacations_DateRange", "[EndDate] >= [StartDate]"));
            });
            #endregion

            #region // ---------- Appointments ----------

            builder.Entity<Appointment>(b =>
            {
                b.HasOne(a => a.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(a => a.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.Property(a => a.Status)
                    .HasConversion<string>()
                    .HasMaxLength(16);


                // Speeds up the doctor's calendar and the overlap check.
                b.HasIndex(a => new { a.DoctorId, a.StartDateTimeUtc });

                b.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Appointments_Status",
                        "[Status] IN ('Pending', 'Confirmed', 'Rejected', 'Cancelled', 'Completed', 'NoShow')");


                    t.HasCheckConstraint(
                        "CK_Appointments_TimeRange",
                        "[EndDateTimeUtc] > [StartDateTimeUtc]");
                });
            });
            #endregion

            #region // ---------- Medical records and prescriptions ----------

            builder.Entity<MedicalRecord>()
                .HasOne(m => m.Appointment)
                .WithOne(a => a.MedicalRecord)
                .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Prescription>(b =>
            {
                b.HasOne(p => p.MedicalRecord)
                    .WithMany(m => m.Prescriptions)
                    .HasForeignKey(p => p.MedicalRecordId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Each version number appears once per medical record.
                b.HasIndex(p => new { p.MedicalRecordId, p.VersionNumber })
                    .IsUnique();
            });

            builder.Entity<PrescriptionItem>()
                .HasOne(i => i.Prescription)
                .WithMany(p => p.PrescriptionItems)
                .HasForeignKey(i => i.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region // ---------- Notifications ----------

            builder.Entity<Notification>(b =>
            {
                b.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.Property(n => n.Type)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });
            #endregion
        }
    }
}
