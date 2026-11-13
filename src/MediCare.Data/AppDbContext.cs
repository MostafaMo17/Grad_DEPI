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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.Email).IsRequired();
                b.Property(u => u.NormalizedEmail).IsRequired();
                b.HasIndex(u => u.NormalizedEmail).IsUnique();
            });

           
            builder.Entity<IdentityUserRole<int>>()
                .HasIndex(ur => ur.UserId)
                .IsUnique();


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
        }
    }
}
