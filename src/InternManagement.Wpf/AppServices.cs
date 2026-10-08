using System;
using System.IO;
using InternManagement.Core.Data;
using InternManagement.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Wpf
{
    /// <summary>
    /// Nơi giữ các service dùng chung cho toàn bộ giao diện.
    /// Mỗi service mở một AppDbContext mới cho từng thao tác rồi đóng lại.
    /// </summary>
    public static class AppServices
    {
        public static AuthService Auth { get; private set; }
        public static DepartmentService Departments { get; private set; }
        public static MentorService Mentors { get; private set; }
        public static InternService Interns { get; private set; }
        public static TaskService Tasks { get; private set; }
        public static EvaluationService Evaluations { get; private set; }
        public static DashboardService Dashboard { get; private set; }

        /// <summary>Tạo CSDL (file internship.db cạnh file .exe), nạp dữ liệu mẫu và khởi tạo service.</summary>
        public static void Initialize()
        {
            string dbFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "internship.db");
            DbContextOptions<AppDbContext> options = AppDbContext.CreateOptions(dbFile);
            Func<AppDbContext> createDb = () => new AppDbContext(options);

            using (AppDbContext db = createDb())
            {
                DbSeeder.Initialize(db);
            }

            Auth = new AuthService(createDb);
            Departments = new DepartmentService(createDb);
            Mentors = new MentorService(createDb);
            Interns = new InternService(createDb);
            Tasks = new TaskService(createDb);
            Evaluations = new EvaluationService(createDb);
            Dashboard = new DashboardService(createDb);
        }
    }
}
