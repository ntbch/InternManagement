using System;
using System.Collections.Generic;
using System.Linq;
using InternManagement.Core.Data;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;

namespace InternManagement.Core.Services
{
    /// <summary>Một cột trong biểu đồ: nhãn + giá trị.</summary>
    public class ChartItem
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    /// <summary>Số liệu tổng hợp cho màn hình Dashboard.</summary>
    public class DashboardSummary
    {
        public int TotalInterns { get; set; }
        public int ActiveInterns { get; set; }
        public int TotalMentors { get; set; }
        public int TotalDepartments { get; set; }
        public int OverdueTasks { get; set; }
        /// <summary>Điểm đánh giá trung bình; null nếu chưa có đánh giá nào.</summary>
        public double? AverageScore { get; set; }
        public List<ChartItem> InternsByStatus { get; set; } = new List<ChartItem>();
        public List<ChartItem> InternsByDepartment { get; set; } = new List<ChartItem>();
    }

    public class DashboardService
    {
        private readonly Func<AppDbContext> _createDb;

        public DashboardService(Func<AppDbContext> createDb)
        {
            _createDb = createDb;
        }

        public DashboardSummary GetSummary()
        {
            using (AppDbContext db = _createDb())
            {
                List<Intern> interns = db.Interns.ToList();
                DateTime today = DateTime.Today;

                var summary = new DashboardSummary
                {
                    TotalInterns = interns.Count,
                    ActiveInterns = interns.Count(i => i.Status == InternStatus.Active),
                    TotalMentors = db.Mentors.Count(),
                    TotalDepartments = db.Departments.Count(),
                    OverdueTasks = db.Tasks.Count(t => t.Status != TaskItemStatus.Done && t.DueDate < today)
                };

                // Average là thuộc tính tính toán (không có trong CSDL) nên tính trên bộ nhớ
                List<Evaluation> evaluations = db.Evaluations.ToList();
                if (evaluations.Count > 0)
                    summary.AverageScore = Math.Round(evaluations.Average(e => e.Average), 2);

                foreach (InternStatus status in Enum.GetValues(typeof(InternStatus)))
                {
                    summary.InternsByStatus.Add(new ChartItem
                    {
                        Label = StatusText.Of(status),
                        Value = interns.Count(i => i.Status == status)
                    });
                }

                foreach (Department d in db.Departments.OrderBy(d => d.Name).ToList())
                {
                    summary.InternsByDepartment.Add(new ChartItem
                    {
                        Label = d.Name,
                        Value = interns.Count(i => i.DepartmentId == d.Id)
                    });
                }

                return summary;
            }
        }
    }
}
