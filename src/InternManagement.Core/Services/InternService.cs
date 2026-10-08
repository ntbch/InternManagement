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
    /// <summary>
    /// Quản lý thực tập sinh. Thêm thực tập sinh sẽ tạo tài khoản: tên đăng nhập = mã TTS, mật khẩu = 123456.
    /// </summary>
    public class InternService
    {
        private const string CodePrefix = "TTS";
        private readonly Func<AppDbContext> _createDb;

        public InternService(Func<AppDbContext> createDb)
        {
            _createDb = createDb;
        }

        /// <summary>Tìm kiếm + lọc. Tham số null nghĩa là không lọc theo tiêu chí đó.</summary>
        public List<Intern> Search(string keyword = null, InternStatus? status = null,
            int? departmentId = null, int? mentorId = null)
        {
            using (AppDbContext db = _createDb())
            {
                IQueryable<Intern> query = db.Interns
                    .Include(i => i.Department)
                    .Include(i => i.Mentor);

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    // LIKE trong SQLite không phân biệt hoa thường (với chữ không dấu)
                    string pattern = "%" + keyword.Trim() + "%";
                    query = query.Where(i => EF.Functions.Like(i.Code, pattern)
                                          || EF.Functions.Like(i.FullName, pattern)
                                          || EF.Functions.Like(i.Email, pattern)
                                          || EF.Functions.Like(i.University, pattern));
                }
                if (status.HasValue)
                    query = query.Where(i => i.Status == status.Value);
                if (departmentId.HasValue)
                    query = query.Where(i => i.DepartmentId == departmentId.Value);
                if (mentorId.HasValue)
                    query = query.Where(i => i.MentorId == mentorId.Value);

                return query.OrderBy(i => i.Code).ToList();
            }
        }

        public Intern GetById(int id)
        {
            using (AppDbContext db = _createDb())
            {
                return db.Interns
                    .Include(i => i.Department)
                    .Include(i => i.Mentor)
                    .FirstOrDefault(i => i.Id == id);
            }
        }

        /// <summary>Sinh mã kế tiếp: TTS001, TTS002, ...</summary>
        public string GenerateNextCode()
        {
            using (AppDbContext db = _createDb())
            {
                int max = 0;
                foreach (string code in db.Interns.Select(i => i.Code).ToList())
                {
                    int number;
                    if (code.StartsWith(CodePrefix) && int.TryParse(code.Substring(CodePrefix.Length), out number) && number > max)
                        max = number;
                }
                return CodePrefix + (max + 1).ToString("D3");
            }
        }

        public void Add(Intern intern)
        {
            using (AppDbContext db = _createDb())
            {
                var entity = new Intern();
                Apply(db, entity, intern);
                db.Interns.Add(entity);
                db.Users.Add(new User
                {
                    Username = entity.Code,
                    PasswordHash = PasswordHasher.Hash(DbSeeder.DefaultPassword),
                    Role = UserRole.Intern,
                    Intern = entity
                });
                db.SaveChanges();
                intern.Id = entity.Id;
            }
        }

        public void Update(Intern intern)
        {
            using (AppDbContext db = _createDb())
            {
                Intern entity = db.Interns.Find(intern.Id);
                if (entity == null)
                    throw new ValidationException("Thực tập sinh không tồn tại.");
                Apply(db, entity, intern);

                // Mã TTS là tên đăng nhập nên đổi mã thì đổi luôn tên đăng nhập
                User account = db.Users.FirstOrDefault(u => u.InternId == entity.Id);
                if (account != null)
                    account.Username = entity.Code;

                db.SaveChanges();
            }
        }

        /// <summary>Xóa thực tập sinh; công việc, đánh giá và tài khoản bị xóa theo (cascade).</summary>
        public void Delete(int id)
        {
            using (AppDbContext db = _createDb())
            {
                Intern entity = db.Interns.Find(id);
                if (entity == null)
                    return;
                db.Interns.Remove(entity);
                db.SaveChanges();
            }
        }

        private static void Apply(AppDbContext db, Intern entity, Intern input)
        {
            string code = Validator.Required(input.Code, "Mã thực tập sinh").ToUpperInvariant();
            string fullName = Validator.Required(input.FullName, "Họ tên");
            string email = Validator.Email(input.Email);
            string phone = Validator.OptionalPhone(input.Phone);

            if (input.EndDate.Date <= input.StartDate.Date)
                throw new ValidationException("Ngày kết thúc phải sau ngày bắt đầu.");
            if (!db.Departments.Any(d => d.Id == input.DepartmentId))
                throw new ValidationException("Vui lòng chọn phòng ban.");
            if (input.MentorId.HasValue)
            {
                Mentor mentor = db.Mentors.Find(input.MentorId.Value);
                if (mentor == null)
                    throw new ValidationException("Mentor không tồn tại.");
                if (mentor.DepartmentId != input.DepartmentId)
                    throw new ValidationException("Mentor phải thuộc cùng phòng ban với thực tập sinh.");
            }
            if (db.Interns.Any(i => i.Id != input.Id && i.Code == code))
                throw new ValidationException("Mã thực tập sinh đã tồn tại.");
            if (db.Interns.Any(i => i.Id != input.Id && i.Email == email))
                throw new ValidationException("Email thực tập sinh đã tồn tại.");
            if (db.Users.Any(u => u.Username.ToUpper() == code && u.InternId != input.Id))
                throw new ValidationException("Mã này đã được dùng làm tên đăng nhập của tài khoản khác.");

            entity.Code = code;
            entity.FullName = fullName;
            entity.Email = email;
            entity.Phone = phone;
            entity.University = Validator.Optional(input.University);
            entity.Major = Validator.Optional(input.Major);
            entity.StartDate = input.StartDate.Date;
            entity.EndDate = input.EndDate.Date;
            entity.Status = input.Status;
            entity.DepartmentId = input.DepartmentId;
            entity.MentorId = input.MentorId;
        }
    }
}
