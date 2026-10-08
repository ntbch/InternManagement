namespace InternManagement.Core.Models
{
    /// <summary>Tài khoản đăng nhập. Mentor/Intern có tài khoản gắn với hồ sơ tương ứng.</summary>
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

        /// <summary>Tên hiển thị trên giao diện.</summary>
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
