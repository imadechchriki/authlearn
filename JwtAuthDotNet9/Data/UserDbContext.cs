using JwtAuthDotNet9.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotNet9.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        // 🔥 Ajoute cette méthode pour mapper correctement Guid vers uuid
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("uuid"); // 👈 ici on force PostgreSQL à utiliser uuid
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
