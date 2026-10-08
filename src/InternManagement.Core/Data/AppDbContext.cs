using InternManagement.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Core.Data
{
    /// <summary>
    /// Cầu nối giữa các lớp Model và CSDL SQLite (Entity Framework Core).
    /// Mỗi DbSet tương ứng với một bảng.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Mentor> Mentors { get; set; }
        public DbSet<Intern> Interns { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }

        /// <summary>Tạo cấu hình kết nối tới file SQLite.</summary>
        public static DbContextOptions<AppDbContext> CreateOptions(string databaseFile)
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=" + databaseFile)
                .Options;
        }

        /// <summary>Cấu hình bảng: độ dài, bắt buộc, duy nhất, khóa ngoại.</summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.Property(x => x.Username).IsRequired().HasMaxLength(100);
                e.Property(x => x.PasswordHash).IsRequired();
                e.HasIndex(x => x.Username).IsUnique();
                // Xóa mentor / thực tập sinh thì xóa luôn tài khoản của họ
                e.HasOne(x => x.Mentor).WithMany().HasForeignKey(x => x.MentorId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Intern).WithMany().HasForeignKey(x => x.InternId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Department>(e =>
            {
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<Mentor>(e =>
            {
                e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Email).IsUnique();
                // Không cho xóa phòng ban khi còn mentor
                e.HasOne(x => x.Department).WithMany(d => d.Mentors)
                    .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Intern>(e =>
            {
                e.Property(x => x.Code).IsRequired().HasMaxLength(20);
                e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Code).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
                e.HasOne(x => x.Department).WithMany(d => d.Interns)
                    .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Mentor).WithMany(m => m.Interns)
                    .HasForeignKey(x => x.MentorId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TaskItem>(e =>
            {
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                // Xóa thực tập sinh thì xóa luôn công việc
                e.HasOne(x => x.Intern).WithMany(i => i.Tasks)
                    .HasForeignKey(x => x.InternId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Evaluation>(e =>
            {
                e.HasOne(x => x.Intern).WithMany(i => i.Evaluations)
                    .HasForeignKey(x => x.InternId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Mentor).WithMany()
                    .HasForeignKey(x => x.MentorId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
