using System.Collections.Generic;

namespace InternManagement.Core.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Mentor> Mentors { get; set; } = new List<Mentor>();
        public List<Intern> Interns { get; set; } = new List<Intern>();

        public override string ToString()
        {
            return Name;
        }
    }
}
