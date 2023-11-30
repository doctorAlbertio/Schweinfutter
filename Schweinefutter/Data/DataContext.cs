using Microsoft.EntityFrameworkCore;

namespace Schweinefutter.Data
{
    /// <summary>
    /// Daten Kontext für die Datenbank, ist quasi Default aus der Vorbild der Dokumentation
    /// </summary>
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(Configuration.GetConnectionString("DemoDatenDB"));
        }
        
        
        public DbSet<FutterEntry> Futter { get; set; }
        public DbSet<TierEntry> Tier { get; set; }
        
        
    }
}