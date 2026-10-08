using System;
using System.Collections.Generic;

namespace InternManagement.Core.Models
{
    /// <summary>Thực tập sinh.</summary>
    public class Intern : Person
    {
        /// <summary>Mã thực tập sinh, ví dụ TTS001. Cũng là tên đăng nhập.</summary>
        public string Code { get; set; }
        public string University { get; set; }
        public string Major { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public InternStatus Status { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public int? MentorId { get; set; }
        public Mentor Mentor { get; set; }

        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public List<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

        public override string RoleName
        {
            get { return "Thực tập sinh"; }
        }

        public override string GetDisplayInfo()
        {
            return Code + " - " + base.GetDisplayInfo();
        }
    }
}
