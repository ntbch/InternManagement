namespace InternManagement.Core.Models
{
    public enum UserRole
    {
        Admin,
        Mentor,
        Intern
    }

    public enum InternStatus
    {
        Pending,
        Active,
        Completed,
        Terminated
    }

    public enum TaskItemStatus
    {
        Todo,
        InProgress,
        Done
    }
}
