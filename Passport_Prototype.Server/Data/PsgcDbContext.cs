using Microsoft.EntityFrameworkCore;
using SeniorCitizen.Server.Models.PSGC;

namespace SeniorCitizen.Server.Data
{
    public class PsgcDbContext : DbContext
    {
        public PsgcDbContext(DbContextOptions<PsgcDbContext> options)
            : base(options)
        {
        }

        // Correct DbSets
        public DbSet<PSGC_Region> Regions => Set<PSGC_Region>();
        public DbSet<PSGC_Province> Provinces => Set<PSGC_Province>();
        public DbSet<PSGC_CityMunicipality> Cities => Set<PSGC_CityMunicipality>();
        public DbSet<PSGC_Barangay> Barangays => Set<PSGC_Barangay>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Region
            modelBuilder.Entity<PSGC_Region>(entity =>
            {
                entity.ToTable("PSGC_Regions");
                entity.HasKey(e => e.RegionCode);
                entity.Property(e => e.RegionCode).HasMaxLength(10);
                entity.Property(e => e.RegionName).HasMaxLength(150);

                entity.HasMany(r => r.Provinces)
                      .WithOne(p => p.Region)
                      .HasForeignKey(p => p.RegionCode);
            });

            // Province
            modelBuilder.Entity<PSGC_Province>(entity =>
            {
                entity.ToTable("PSGC_Provinces");
                entity.HasKey(e => e.ProvinceCode);
                entity.Property(e => e.ProvinceCode).HasMaxLength(10);
                entity.Property(e => e.ProvinceName).HasMaxLength(150);

                entity.HasMany(p => p.Cities)
                      .WithOne(c => c.Province)
                      .HasForeignKey(c => c.ProvinceCode);
            });

            // City/Municipality
            modelBuilder.Entity<PSGC_CityMunicipality>(entity =>
            {
                entity.ToTable("PSGC_CitiesMunicipalities");
                entity.HasKey(e => e.CityMunCode);
                entity.Property(e => e.CityMunCode).HasMaxLength(10);
                entity.Property(e => e.CityMunName).HasMaxLength(150);

                entity.HasMany(c => c.Barangays)
                      .WithOne(b => b.CityMunicipality)
                      .HasForeignKey(b => b.CityMunCode);
            });

            // Barangay
            modelBuilder.Entity<PSGC_Barangay>(entity =>
            {
                entity.ToTable("PSGC_Barangays");
                entity.HasKey(e => e.BarangayCode);
                entity.Property(e => e.BarangayCode).HasMaxLength(10);
                entity.Property(e => e.BarangayName).HasMaxLength(150);
            });
        }
    }
}