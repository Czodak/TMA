using AuthApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.Id)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            base.OnModelCreating(modelBuilder); 
        }
    }
}
