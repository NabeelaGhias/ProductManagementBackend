using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.Domain.Entities;

namespace ProductManagementSystem.Domain.Data
{
    public class DataContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Add soft delete query filter
            builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        }
    }
}