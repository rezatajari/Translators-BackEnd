using Microsoft.EntityFrameworkCore;
using Translators.Domain.Users;

namespace Translators.EntityFrameworkCore.DbContexts;

public class TranslatorDbContext : DbContext
{
    public TranslatorDbContext(DbContextOptions<TranslatorDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<RoleEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.Permissions)
                .HasMaxLength(2000);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<RoleEntity>().HasData(
            new RoleEntity
            {
                Id = adminRoleId,
                Name = RoleNames.Admin,
                Description = "System administrator with full access.",
                Permissions = "*",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            },
            new RoleEntity
            {
                Id = userRoleId,
                Name = RoleNames.User,
                Description = "Default registered user with read-only access.",
                Permissions = "GetByIdUserHandler,PaginatedUserHandler,LookupUserHandler",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.RoleId)
                .IsRequired();

            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
