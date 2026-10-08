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
    /// <summary>Quản lý mentor. Thêm mentor sẽ tạo tài khoản: tên đăng nhập = email, mật khẩu = 123456.</summary>
    public class MentorService
    {
        private readonly Func<AppDbContext> _createDb;

        public MentorService(Func<AppDbContext> createDb)
        {
            _createDb = createDb;
        }

        public List<Mentor> GetAll(string keyword = null, int? departmentId = null)
        {
            using (AppDbContext db = _createDb())
            {
                IQueryable<Mentor> query = db.Mentors
                    .Include(m => m.Department)
                    .Include(m => m.Interns);

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    string pattern = "%" + keyword.Trim() + "%";
                    query = query.Where(m => EF.Functions.Like(m.FullName, pattern)
                                          || EF.Functions.Like(m.Email, pattern));
                }
                if (departmentId.HasValue)
                    query = query.Where(m => m.DepartmentId == departmentId.Value);

                return query.OrderBy(m => m.FullName).ToList();
            }
        }

        public void Add(Mentor mentor)
        {
            using (AppDbContext db = _createDb())
            {
                var entity = new Mentor();
                Apply(db, entity, mentor);
                db.Mentors.Add(entity);
                db.Users.Add(new User
                {
                    Username = entity.Email,
                    PasswordHash = PasswordHasher.Hash(DbSeeder.DefaultPassword),
                    Role = UserRole.Mentor,
                    Mentor = entity
                });
                db.SaveChanges();
                mentor.Id = entity.Id;
            }
        }

        public void Update(Mentor mentor)
        {
            using (AppDbContext db = _createDb())
            {
                Mentor entity = db.Mentors.Find(mentor.Id);
                if (entity == null)
                    throw new ValidationException("Mentor không tồn tại.");
                Apply(db, entity, mentor);

                // Email là tên đăng nhập nên đổi email thì đổi luôn tên đăng nhập
                User account = db.Users.FirstOrDefault(u => u.MentorId == entity.Id);
                if (account != null)
                    account.Username = entity.Email;

                db.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (AppDbContext db = _createDb())
            {
                Mentor entity = db.Mentors.Find(id);
                if (entity == null)
                    return;
                if (db.Interns.Any(i => i.MentorId == id))
                    throw new ValidationException("Mentor đang hướng dẫn thực tập sinh. Hãy chuyển thực tập sinh sang mentor khác trước.");
                if (db.Evaluations.Any(e => e.MentorId == id))
                    throw new ValidationException("Mentor đã có phiếu đánh giá nên không thể xóa.");
                db.Mentors.Remove(entity); // tài khoản bị xóa theo (cascade)
                db.SaveChanges();
            }
        }

        private static void Apply(AppDbContext db, Mentor entity, Mentor input)
        {
            string fullName = Validator.Required(input.FullName, "Họ tên");
            string email = Validator.Email(input.Email);
            string phone = Validator.OptionalPhone(input.Phone);
            if (!db.Departments.Any(d => d.Id == input.DepartmentId))
                throw new ValidationException("Vui lòng chọn phòng ban.");
            if (db.Mentors.Any(m => m.Id != input.Id && m.Email == email))
                throw new ValidationException("Email mentor đã tồn tại.");
            if (db.Users.Any(u => u.Username == email && u.MentorId != input.Id))
                throw new ValidationException("Email đã được dùng làm tên đăng nhập của tài khoản khác.");

            entity.FullName = fullName;
            entity.Email = email;
            entity.Phone = phone;
            entity.Position = Validator.Optional(input.Position);
            entity.DepartmentId = input.DepartmentId;
        }
    }
}
