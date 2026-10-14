using InternManagement.Core.Models;

namespace InternManagement.Wpf
{
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

        public static int? MentorId
        {
            get { return CurrentUser == null ? null : CurrentUser.MentorId; }
        }

        public static int? InternId
        {
            get { return CurrentUser == null ? null : CurrentUser.InternId; }
        }
    }
}
