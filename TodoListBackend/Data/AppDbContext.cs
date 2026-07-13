using Microsoft.EntityFrameworkCore;
using TodoListBackend.Models;

namespace TodoListBackend.Data 
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Todo> Todos => Set<Todo>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<SubTask> SubTasks => Set<SubTask>();
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

            modelBuilder.Entity<SubTask>(entity =>
            {
                entity.Property(s => s.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.HasOne(s => s.Todo)
                    .WithMany(t => t.SubTasks)
                    .HasForeignKey(s => s.TodoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => s.TodoId)
                      .HasDatabaseName("IX_SubTasks_TodoId");
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

            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(p => p.Name)
                    .HasMaxLength(100) 
                    .IsRequired();
                entity.Property(p => p.Color)
                    .HasMaxLength(30);
                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.UserId)
                      .HasDatabaseName("IX_Projects_UserId");
            });
        }
    }
}