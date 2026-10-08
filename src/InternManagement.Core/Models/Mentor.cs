using System.Collections.Generic;

namespace InternManagement.Core.Models
{
    /// <summary>Người hướng dẫn thực tập.</summary>
    public class Mentor : Person
    {
        public string Position { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public List<Intern> Interns { get; set; } = new List<Intern>();

        public override string RoleName
        {
            get { return "Mentor"; }
        }
    }
}
