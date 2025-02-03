using Microsoft.EntityFrameworkCore;
using FreezerManager.Models;

namespace FreezerManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<MeatItem> MeatItems {get; set;}
    }
}
