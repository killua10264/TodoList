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
                    .HasMaxLength(200) // nvarchar(200)
                    .IsRequired(); //NOT NULL

                entity.Property(t => t.Description)
                    .HasMaxLength(1000); // nvarchar(1000)
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
            });

            // Cấu hình cho bảng Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name)
                    .HasMaxLength(100) 
                    .IsRequired();
                entity.Property(c => c.Color)
                    .HasMaxLength(30);
            });
        }
    }
}