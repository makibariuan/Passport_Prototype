
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using SeniorCitizen.Server.DTOs;
using SeniorCitizen.Server.Models;

namespace OnlineRegistration.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UsersEmployee> EmployeeUsers { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<CitizenIDApplication> CitizenIDApplications { get; set; }
        public DbSet<EmployeeIDApplication> EmployeeIDApplications { get; set; }
        public DbSet<BiometricDataEnrollment> BiometricEnrollments { get; set; }
        public DbSet<BypassLog> BypassLogs { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<CitizenVerificationLog>VerificationLogs { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<PersonalInformation> PersonalInformations { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AppointmentQuota> AppointmentQuotas { get; set; }

        public DbSet<AttendanceLogProcessed> ProcessedLogs { get; set; }
        public DbSet<EnrollmentRegistryID> EnrollmentRegistries { get; set; }


        //stored procedures
        public DbSet<UserReadDto> UserReadDtos { get; set; }
        public DbSet<ApplicantReadDto> ApplicantReadDtos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This is the missing piece:
            modelBuilder.Entity<AppointmentQuota>()
                .HasKey(q => new { q.SiteName, q.ScheduledDate, q.ScheduledTime });


            //storeed procedure mapping
            modelBuilder.Entity<UserReadDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<ApplicantReadDto>().HasNoKey().ToView(null);

            base.OnModelCreating(modelBuilder);
        }
    }
}
