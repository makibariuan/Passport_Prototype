using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Models;
using SeniorCitizen.Server.Models;

namespace OnlineRegistration.Server.Data
{
    public class EmployeesDbContext : DbContext
    {
        public EmployeesDbContext(DbContextOptions<EmployeesDbContext> options) : base(options) { }

        // --- Core HR & Lookup Tables ---
        public DbSet<PersonalInformation> PersonalInformations { get; set; }
        public DbSet<BiometricDataEnrollment> BiometricEnrollments { get; set; }
        public DbSet<CivilServiceEligibility> CivilServiceEligibilities { get; set; }
        public DbSet<VoluntaryWork> VoluntaryWorks { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<SkillHobby> SkillsHobbies { get; set; }
        public DbSet<Distinction> Distinctions { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<EmployeeIDApplication> EmployeeIDApplications { get; set; }
        public DbSet<PdsSectionIV> PdsSectionIVs { get; set; }
        public DbSet<Gender> Gender { get; set; }
        public DbSet<CivilStatus> CivilStatus { get; set; }
        public DbSet<BloodType> BloodType { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<EducationLevel> EducationLevels { get; set; }
        public DbSet<EmployeeAttendance> Attendance { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<PersonalInformationReference> PersonalInformationReferences { get; set; }
        public DbSet<PdsSectionVDeclaration> PdsSectionVDeclarations { get; set; }
        public DbSet<AttendanceLogs_Processed> AttendanceLogs_Processed { get; set; }
        public DbSet<Ref_DaysOfWeek> Ref_DaysOfWeek { get; set; }
        public DbSet<EnrollmentRegistryID> EnrollmentRegistries { get; set; }

        // --- New Hierarchical Scheduling Tables ---
        public DbSet<ScheduleTemplate> ScheduleTemplates { get; set; }
        public DbSet<DaySchedule> DaySchedules { get; set; }
        public DbSet<ShiftSegment> ShiftSegments { get; set; }
        public DbSet<EmployeeScheduleAssignment> EmployeeScheduleAssignments { get; set; }

        public DbSet<DailyTimeRecord> DailyTimeRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. HR Entity Table Mappings
            modelBuilder.Entity<EmployeeAttendance>().ToTable("EmployeeAttendance");
            modelBuilder.Entity<PersonalInformation>().ToTable("PersonalInformation").HasKey(p => p.PersonID);
            modelBuilder.Entity<Child>().ToTable("PersonalInformation_Children");
            modelBuilder.Entity<Education>(entity => {
                entity.ToTable("PersonalInformation_Education");
                entity.HasKey(e => e.EducationId);
            });
            modelBuilder.Entity<CivilServiceEligibility>().ToTable("PersonalInformation_CivilServiceEligibility").HasKey(e => e.EligibilityID);
            modelBuilder.Entity<VoluntaryWork>().ToTable("PersonalInformation_VoluntaryWork").HasKey(v => v.WorkId);
            modelBuilder.Entity<Training>().ToTable("PersonalInformation_Training").HasKey(t => t.TrainingId);
            modelBuilder.Entity<SkillHobby>().ToTable("PersonalInformation_SkillsHobbies").HasKey(s => s.SkillId);
            modelBuilder.Entity<Distinction>().ToTable("PersonalInformation_Distinctions").HasKey(d => d.DistinctionId);
            modelBuilder.Entity<Membership>().ToTable("PersonalInformation_Memberships").HasKey(m => m.MembershipId);
            modelBuilder.Entity<PdsSectionIV>().ToTable("PersonalInformation_C4");
            modelBuilder.Entity<Gender>().ToTable("Gender").HasKey(r => r.GenderID);
            modelBuilder.Entity<CivilStatus>().ToTable("CivilStatus").HasKey(r => r.CivilStatusID);
            modelBuilder.Entity<Department>().ToTable("Department").HasKey(r => r.DepartmentID);

            modelBuilder.Entity<EducationLevel>(entity => {
                entity.ToTable("EducationLevels");
                entity.HasKey(e => e.Level_ID);
                entity.Property(e => e.Level_ID).HasColumnName("Level_ID");
                entity.Property(e => e.Display_Name).HasColumnName("Display_Name");
                entity.Property(e => e.Code).HasColumnName("Code");
            });

            modelBuilder.Entity<Employee>().ToTable("Employee");
            modelBuilder.Entity<BiometricDevice>().HasKey(p => p.BiometricDeviceID);
            modelBuilder.Entity<EmployeeAttendance>().HasKey(p => new { p.EmployeeID, p.BiometricDeviceID, p.DateLog });
            modelBuilder.Entity<PdsSectionVDeclaration>().ToTable("PersonalInformation_Declaration");
            modelBuilder.Entity<PersonalInformationReference>().ToTable("PersonalInformation_References");

            // 2. HR Relationships
            modelBuilder.Entity<EmployeeAttendance>().HasOne(a => a.BiometricDevice).WithMany().HasForeignKey(a => a.BiometricDeviceID);

            // 3. Hierarchical Scheduling Mappings (Ensures no legacy column creation)
            modelBuilder.Entity<ScheduleTemplate>(entity => {
                entity.ToTable("ScheduleTemplates");
                entity.HasKey(e => e.TemplateId);
            });

            modelBuilder.Entity<DaySchedule>(entity => {
                entity.ToTable("DaySchedules");
                entity.HasKey(d => d.DayScheduleId);
                entity.HasOne(d => d.Template)
                      .WithMany(t => t.Days)
                      .HasForeignKey(d => d.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShiftSegment>(entity => {
                entity.ToTable("ShiftSegments");
                entity.HasKey(s => s.SegmentId);
                entity.HasOne(s => s.DaySchedule)
                      .WithMany(d => d.ShiftSegments)
                      .HasForeignKey(s => s.DayScheduleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EmployeeScheduleAssignment>(entity => {
                entity.ToTable("EmployeeScheduleAssignments");
                entity.HasKey(e => e.AssignmentId);
                entity.HasOne(e => e.Template)
                      .WithMany()
                      .HasForeignKey(e => e.TemplateId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AttendanceLogs_Processed>(entity => {
                entity.ToTable("AttendanceLogs_Processed");
                entity.HasKey(e => e.ProcessedId);
                
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}