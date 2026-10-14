namespace InternManagement.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }

        public int? MentorId { get; set; }
        public Mentor Mentor { get; set; }

        public int? InternId { get; set; }
        public Intern Intern { get; set; }

        public string DisplayName
        {
            get
            {
                if (Mentor != null) return Mentor.FullName;
                if (Intern != null) return Intern.FullName;
                return Username;
            }
        }
    }
}
