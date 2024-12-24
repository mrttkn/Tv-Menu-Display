using Microsoft.EntityFrameworkCore;
using MenuDisplayApp.Models;

namespace MenuDisplayApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MenuItem>().HasNoKey(); // View veya sorgu sonuçları için
        }
    }
}