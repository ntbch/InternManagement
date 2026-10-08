using InternManagement.Core.Models;

namespace InternManagement.Core.Helpers
{
    /// <summary>Chuyển giá trị enum sang chữ tiếng Việt để hiển thị.</summary>
    public static class StatusText
    {
        public static string Of(InternStatus status)
        {
            switch (status)
            {
                case InternStatus.Pending: return "Chờ bắt đầu";
                case InternStatus.Active: return "Đang thực tập";
                case InternStatus.Completed: return "Hoàn thành";
                case InternStatus.Terminated: return "Dừng thực tập";
                default: return status.ToString();
            }
        }

        public static string Of(TaskItemStatus status)
        {
            switch (status)
            {
                case TaskItemStatus.Todo: return "Chưa làm";
                case TaskItemStatus.InProgress: return "Đang làm";
                case TaskItemStatus.Done: return "Hoàn thành";
                default: return status.ToString();
            }
        }

        public static string Of(UserRole role)
        {
            switch (role)
            {
                case UserRole.Admin: return "Quản trị viên";
                case UserRole.Mentor: return "Mentor";
                case UserRole.Intern: return "Thực tập sinh";
                default: return role.ToString();
            }
        }
    }
}
