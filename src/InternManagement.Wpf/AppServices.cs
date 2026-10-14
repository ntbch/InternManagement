using System;
using System.IO;
using InternManagement.Core.Data;
using InternManagement.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Wpf
{
    public static class AppServices
    {
        public static AuthService Auth { get; private set; }
        public static DepartmentService Departments { get; private set; }
        public static MentorService Mentors { get; private set; }
        public static InternService Interns { get; private set; }
        public static TaskService Tasks { get; private set; }
        public static EvaluationService Evaluations { get; private set; }
        public static DashboardService Dashboard { get; private set; }

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
