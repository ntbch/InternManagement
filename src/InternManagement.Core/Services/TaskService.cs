using System;
using System.Collections.Generic;
using System.Linq;
using InternManagement.Core.Data;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Core.Services
{
    /// <summary>Giao việc cho thực tập sinh và cập nhật tiến độ.</summary>
    public class TaskService
    {
        private readonly Func<AppDbContext> _createDb;

        public TaskService(Func<AppDbContext> createDb)
        {
            _createDb = createDb;
        }

        /// <summary>
        /// Lấy công việc. internId: chỉ của 1 thực tập sinh; mentorId: của các thực tập sinh do mentor hướng dẫn.
        /// </summary>
        public List<TaskItem> GetTasks(int? internId = null, int? mentorId = null, TaskItemStatus? status = null)
        {
            using (AppDbContext db = _createDb())
            {
                IQueryable<TaskItem> query = db.Tasks.Include(t => t.Intern);

                if (internId.HasValue)
                    query = query.Where(t => t.InternId == internId.Value);
                if (mentorId.HasValue)
                    query = query.Where(t => t.Intern.MentorId == mentorId.Value);
                if (status.HasValue)
                    query = query.Where(t => t.Status == status.Value);

                return query.OrderBy(t => t.DueDate).ToList();
            }
        }

        public void Add(TaskItem task)
        {
            using (AppDbContext db = _createDb())
            {
                var entity = new TaskItem();
                Apply(db, entity, task);
                db.Tasks.Add(entity);
                db.SaveChanges();
                task.Id = entity.Id;
            }
        }

        public void Update(TaskItem task)
        {
            using (AppDbContext db = _createDb())
            {
                TaskItem entity = db.Tasks.Find(task.Id);
                if (entity == null)
                    throw new ValidationException("Công việc không tồn tại.");
                Apply(db, entity, task);
                db.SaveChanges();
            }
        }

        /// <summary>Thực tập sinh cập nhật trạng thái công việc của chính mình.</summary>
        public void UpdateStatus(int taskId, TaskItemStatus status, int internId)
        {
            using (AppDbContext db = _createDb())
            {
                TaskItem entity = db.Tasks.Find(taskId);
                if (entity == null)
                    throw new ValidationException("Công việc không tồn tại.");
                if (entity.InternId != internId)
                    throw new ValidationException("Bạn chỉ được cập nhật công việc của mình.");
                entity.Status = status;
                db.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (AppDbContext db = _createDb())
            {
                TaskItem entity = db.Tasks.Find(id);
                if (entity == null)
                    return;
                db.Tasks.Remove(entity);
                db.SaveChanges();
            }
        }

        private static void Apply(AppDbContext db, TaskItem entity, TaskItem input)
        {
            string title = Validator.Required(input.Title, "Tiêu đề công việc");
            if (!db.Interns.Any(i => i.Id == input.InternId))
                throw new ValidationException("Vui lòng chọn thực tập sinh.");
            if (input.DueDate.Date < input.AssignedDate.Date)
                throw new ValidationException("Hạn hoàn thành không được trước ngày giao.");

            entity.Title = title;
            entity.Description = Validator.Optional(input.Description);
            entity.AssignedDate = input.AssignedDate.Date;
            entity.DueDate = input.DueDate.Date;
            entity.Status = input.Status;
            entity.InternId = input.InternId;
        }
    }
}
