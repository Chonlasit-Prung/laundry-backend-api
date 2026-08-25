using Microsoft.EntityFrameworkCore;
using LaundryApi.Models;

namespace LaundryApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Setting> Settings { get; set; }
    }
}