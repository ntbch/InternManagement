namespace InternManagement.Core.Models
{
    /// <summary>Vai trò của tài khoản đăng nhập.</summary>
    public enum UserRole
    {
        Admin,
        Mentor,
        Intern
    }

    /// <summary>Trạng thái thực tập.</summary>
    public enum InternStatus
    {
        Pending,     // Chờ bắt đầu
        Active,      // Đang thực tập
        Completed,   // Hoàn thành
        Terminated   // Dừng thực tập
    }

    /// <summary>Trạng thái công việc được giao.</summary>
    public enum TaskItemStatus
    {
        Todo,        // Chưa làm
        InProgress,  // Đang làm
        Done         // Hoàn thành
    }
}
