using Microsoft.EntityFrameworkCore;
using WebServer.Models;  // Adjust this to match your actual namespace


namespace WebServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for Clients table
        public DbSet<Client> Clients { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .HasKey(c => c.ClientID);

            modelBuilder.Entity<Client>()
                .Property(c => c.ClientID)
                .ValueGeneratedOnAdd(); // This ensures auto-generation

            base.OnModelCreating(modelBuilder);
        }
    }
}
