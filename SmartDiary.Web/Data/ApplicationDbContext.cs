using Microsoft.EntityFrameworkCore;
using SmartDiary.Web.Models;
using Task = SmartDiary.Web.Models.Task;

namespace SmartDiary.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        // Переопределение метода SaveChanges для автоматического обновления UpdatedAt
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        // Метод для обновления поля UpdatedAt у измененных и добавленных записей
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is User || e.Entity is Project || e.Entity is Tag || e.Entity is Task);

            foreach (var entityEntry in entries)
            {
                // Для измененных записей
                if (entityEntry.State == EntityState.Modified)
                {
                    // Получаем свойство UpdatedAt через рефлексию или динамическое обращение
                    var property = entityEntry.Entity.GetType().GetProperty("UpdatedAt");
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(entityEntry.Entity, DateTime.UtcNow);
                    }
                }

                // Для добавленных записей (если нужно устанавливать UpdatedAt при создании)
                if (entityEntry.State == EntityState.Added)
                {
                    var property = entityEntry.Entity.GetType().GetProperty("UpdatedAt");
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(entityEntry.Entity, DateTime.UtcNow);
                    }
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.Name, t.OwnerId })
                .IsUnique();

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tag>()
                .HasOne(t => t.Owner)
                .WithMany(u => u.Tags)
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new { tt.TaskId, tt.TagId });

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Task)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Task>()
                .HasIndex(t => t.Status)
                .HasDatabaseName("IX_Task_Status");

            modelBuilder.Entity<Task>()
                .HasIndex(t => t.Deadline)
                .HasDatabaseName("IX_Task_Deadline");

            modelBuilder.Entity<Task>()
                .HasIndex(t => new { t.UserId, t.Status })
                .HasDatabaseName("IX_Task_UserId_Status");
        }
    }
}