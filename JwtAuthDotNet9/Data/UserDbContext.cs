using JwtAuthDotNet9.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotNet9.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapping standard UUID
            modelBuilder.HasPostgresExtension("uuid-ossp"); // si tu veux aussi utiliser gen_random_uuid()

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("uuid");
                entity.HasMany(u => u.UserRoles)
                      .WithOne(ur => ur.User)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.RefreshTokens)
                      .WithOne(rt => rt.User)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(u => u.Profile)
                      .WithOne(p => p.User)
                      .HasForeignKey<UserProfile>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("uuid");
                entity.HasMany(r => r.UserRoles)
                      .WithOne(ur => ur.Role)
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("uuid");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("uuid");
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("uuid");
                entity.HasIndex(p => p.UserId).IsUnique(); // 1 User → 1 Profile
            });
        }
        public static async Task SeedAsync(UserDbContext context)
        {
            if (!await context.Roles.AnyAsync())
            {
                var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
                var studentRole = new Role { Id = Guid.NewGuid(), Name = "Student" };
                var teacherRole = new Role { Id = Guid.NewGuid(), Name = "Teacher" };

                await context.Roles.AddRangeAsync(adminRole, studentRole, teacherRole);
                await context.SaveChangesAsync();
            }

            if (!await context.Users.AnyAsync())
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "admin@admin.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var passwordHasher = new PasswordHasher<User>();
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");

                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");

                adminUser.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
        }


    }



}
