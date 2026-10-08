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
    public class DepartmentService
    {
        private readonly Func<AppDbContext> _createDb;

        public DepartmentService(Func<AppDbContext> createDb)
        {
            _createDb = createDb;
        }

        /// <summary>Danh sách phòng ban kèm mentor và thực tập sinh (dùng cho bảng và TreeView).</summary>
        public List<Department> GetAll()
        {
            using (AppDbContext db = _createDb())
            {
                return db.Departments
                    .Include(d => d.Mentors).ThenInclude(m => m.Interns)
                    .Include(d => d.Interns)
                    .OrderBy(d => d.Name)
                    .ToList();
            }
        }

        public void Add(Department department)
        {
            using (AppDbContext db = _createDb())
            {
                var entity = new Department();
                Apply(db, entity, department);
                db.Departments.Add(entity);
                db.SaveChanges();
                department.Id = entity.Id;
            }
        }

        public void Update(Department department)
        {
            using (AppDbContext db = _createDb())
            {
                Department entity = db.Departments.Find(department.Id);
                if (entity == null)
                    throw new ValidationException("Phòng ban không tồn tại.");
                Apply(db, entity, department);
                db.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (AppDbContext db = _createDb())
            {
                Department entity = db.Departments.Find(id);
                if (entity == null)
                    return;
                if (db.Mentors.Any(m => m.DepartmentId == id) || db.Interns.Any(i => i.DepartmentId == id))
                    throw new ValidationException("Không thể xóa phòng ban đang có mentor hoặc thực tập sinh.");
                db.Departments.Remove(entity);
                db.SaveChanges();
            }
        }

        private static void Apply(AppDbContext db, Department entity, Department input)
        {
            string name = Validator.Required(input.Name, "Tên phòng ban");
            string lower = name.ToLower();
            if (db.Departments.Any(d => d.Id != input.Id && d.Name.ToLower() == lower))
                throw new ValidationException("Tên phòng ban đã tồn tại.");

            entity.Name = name;
            entity.Description = Validator.Optional(input.Description);
        }
    }
}
