using Microsoft.EntityFrameworkCore;
using TodoListBackend.Models;

namespace TodoListBackend.Data 
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Todo> Todos => Set<Todo>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Todo>(entity => 
            {
                entity.Property(t => t.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(t => t.Description)
                    .HasMaxLength(1000);

                entity.HasIndex(t => new { t.UserId, t.IsDeleted })
                      .HasDatabaseName("IX_Todos_UserId_IsDeleted");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username)
                    .HasMaxLength(50)
                    .IsRequired();
                entity.Property(u => u.Email)
                    .HasMaxLength(150)
                    .IsRequired();
                entity.Property(u => u.Password)
                    .HasMaxLength(255)
                    .IsRequired();
                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(UserRole.User);
                entity.Property(u => u.RefreshToken)
                    .HasMaxLength(255);
                entity.HasIndex(u => u.RefreshToken)
                    .HasDatabaseName("IX_Users_RefreshToken");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name)
                    .HasMaxLength(100) 
                    .IsRequired();
                entity.Property(c => c.Color)
                    .HasMaxLength(30);
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // FIX 2.3: Index cho Category.UserId
                entity.HasIndex(c => c.UserId)
                      .HasDatabaseName("IX_Categories_UserId");
            });
        }
    }
}