using Microsoft.EntityFrameworkCore;
using DailyReport.Models.WardsModels;
using DailyReport.Models.Reports;
using DailyReport.Models.PersonelFolder;

namespace DailyReport.Models
{
    public class ApplicationContext : DbContext
    {

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            //Database.EnsureDeleted();   // удаляем бд со старой схемой
            //Database.EnsureCreated();   // создаем базу данных при первом обращении
        }
        public DbSet<DepReport> DepReports { get; set; } = null!;
        public DbSet<FinalReport> FinalReports { get; set; } = null!;
        public DbSet<DutyDoc> DutyDocs { get; set; } = null!;
        public DbSet<OutcomingPatient> OutcomingPatients { get; set; } = null!;
        public DbSet<Personel> Personels { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<DepartmentSettings> DepartmentSettings { get; set; } = null!;
        public DbSet<DutyNurse> DutyNurses { get; set; } = null!;
        public DbSet<FireReport> FireReports { get; set; } = null!;
        public DbSet<Ward> Wards { get; set; } = null!;
        public DbSet<Patient> Patients  { get; set; } = null!;
        //public DbSet<LineEntity> Lines { get; set; } = null!;
        //public DbSet<LineType> LineTypes { get; set; } = null!;
        //public DbSet<ReportLine> ReportLines { get; set; } = null!;

        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlite(@"Data Source=departmentsdb.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepartmentSettings>(entity =>
            {
                entity.ToTable("DepartmentSettings");
                entity.HasKey(e => e.DepartmentId);
                // enum ���࠭���� � �� ��� �᫮
                entity.Property(e => e.Type).HasConversion<int>();
                entity.HasOne(e => e.Department)
                      .WithOne(d => d.Settings)
                      .HasForeignKey<DepartmentSettings>(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DepReport>(entity =>
            {
                entity.ToTable("DepReports");
                // Одна сводка на отделение в сутки. В SQLite значения NULL в уникальном
                // индексе считаются различными, поэтому нерассчитанные служебные строки
                // с reportDate IS NULL индексу не мешают.
                entity.HasIndex(e => new { e.depNumber, e.reportDate }).IsUnique();
            });
        }

    }
}
