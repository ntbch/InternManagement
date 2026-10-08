using System;

namespace InternManagement.Core.Models
{
    /// <summary>Công việc mentor giao cho thực tập sinh.</summary>
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime DueDate { get; set; }
        public TaskItemStatus Status { get; set; }

        public int InternId { get; set; }
        public Intern Intern { get; set; }

        /// <summary>Quá hạn: chưa xong mà đã qua hạn chót. Không lưu vào CSDL.</summary>
        public bool IsOverdue
        {
            get { return Status != TaskItemStatus.Done && DueDate.Date < DateTime.Today; }
        }
    }
}
