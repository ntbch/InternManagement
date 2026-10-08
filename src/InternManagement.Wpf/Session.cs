using InternManagement.Core.Models;

namespace InternManagement.Wpf
{
    /// <summary>Tài khoản đang đăng nhập.</summary>
    public static class Session
    {
        public static User CurrentUser { get; set; }

        public static bool IsAdmin
        {
            get { return CurrentUser != null && CurrentUser.Role == UserRole.Admin; }
        }

        public static bool IsMentor
        {
            get { return CurrentUser != null && CurrentUser.Role == UserRole.Mentor; }
        }

        public static bool IsIntern
        {
            get { return CurrentUser != null && CurrentUser.Role == UserRole.Intern; }
        }

        /// <summary>Id mentor của tài khoản hiện tại (null nếu không phải mentor).</summary>
        public static int? MentorId
        {
            get { return CurrentUser == null ? null : CurrentUser.MentorId; }
        }

        /// <summary>Id thực tập sinh của tài khoản hiện tại (null nếu không phải thực tập sinh).</summary>
        public static int? InternId
        {
            get { return CurrentUser == null ? null : CurrentUser.InternId; }
        }
    }
}
