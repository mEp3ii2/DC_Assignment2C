using Microsoft.EntityFrameworkCore;
using WebServer.Models;  


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
                .ValueGeneratedOnAdd(); 

            base.OnModelCreating(modelBuilder);
        }
    }
}
