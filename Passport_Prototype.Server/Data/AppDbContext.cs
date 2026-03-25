using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using Passport_Prototype.Server.Models;
using SeniorCitizen.Server.DTOs;
using SeniorCitizen.Server.Models;

namespace OnlineRegistration.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- CORE TABLES ---
        public DbSet<Users> Users { get; set; }
        public DbSet<UsersEmployee> EmployeeUsers { get; set; }
        public DbSet<Employee> Employees { get; set; }

        // --- PASSPORT PROTOTYPE TABLES ---
        public DbSet<PassportPersonalInformation> PassportPersonalInformation { get; set; }
        public DbSet<Family> Family { get; set; }
        public DbSet<Application> Applications { get; set; }

        // --- LEGACY/OTHER TABLES ---
        public DbSet<CitizenIDApplication> CitizenIDApplications { get; set; }
        public DbSet<EmployeeIDApplication> EmployeeIDApplications { get; set; }
        public DbSet<BiometricDataEnrollment> BiometricEnrollments { get; set; }
        public DbSet<BypassLog> BypassLogs { get; set; }
        public DbSet<CitizenVerificationLog> VerificationLogs { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<PersonalInformation> PersonalInformations { get; set; }
        public DbSet<AppointmentQuota> AppointmentQuotas { get; set; }
        public DbSet<AttendanceLogProcessed> ProcessedLogs { get; set; }
        public DbSet<EnrollmentRegistryID> EnrollmentRegistries { get; set; }
        public DbSet<ContactInformation> ContactInformation { get; set; }
        public DbSet<WorkInformation> WorkInformation { get; set; }

        // --- DTOs FOR STORED PROCEDURES ---
        public DbSet<UserReadDto> UserReadDtos { get; set; }
        public DbSet<ApplicantReadDto> ApplicantReadDtos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Appointment Quota Composite Key
            modelBuilder.Entity<AppointmentQuota>()
                .HasKey(q => new { q.SiteName, q.ScheduledDate, q.ScheduledTime });

            // 2. KEYLESS ENTITY MAPPINGS (Add these back!)
            modelBuilder.Entity<UserReadDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<ApplicantReadDto>().HasNoKey().ToView(null);

            // 3. PASSPORT RELATIONSHIP (One-to-One)
            modelBuilder.Entity<Users>()
                .HasOne(u => u.PassportInfo)
                .WithOne()
                .HasForeignKey<PassportPersonalInformation>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}