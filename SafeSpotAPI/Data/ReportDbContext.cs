using Microsoft.EntityFrameworkCore;
using SafeSpotAPI.Models; 

namespace SafeSpotAPI.Data
{
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) { }

        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportValidated> ValidatedReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReportValidated>().ToTable("ValidatedReports");
        }
    }
}
