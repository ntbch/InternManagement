using System;
using System.Linq;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;

namespace InternManagement.Core.Data
{
    /// <summary>
    /// Tạo CSDL (nếu chưa có) và thêm dữ liệu mẫu để demo.
    /// Tài khoản mẫu: admin/admin123; mentor đăng nhập bằng email; thực tập sinh bằng mã TTS; mật khẩu mặc định 123456.
    /// </summary>
    public static class DbSeeder
    {
        public const string DefaultPassword = "123456";

        public static void Initialize(AppDbContext db)
        {
            db.Database.EnsureCreated();

            if (db.Users.Any())
                return; // Đã có dữ liệu

            DateTime today = DateTime.Today;

            var dev = new Department { Name = "Phát triển phần mềm", Description = "Lập trình ứng dụng web, desktop" };
            var qa = new Department { Name = "Kiểm thử", Description = "Kiểm thử phần mềm" };
            var design = new Department { Name = "Thiết kế UI/UX", Description = "Thiết kế giao diện" };
            db.Departments.AddRange(dev, qa, design);

            var mAn = new Mentor { FullName = "Nguyễn Văn An", Email = "an@congty.vn", Phone = "0912345678", Position = "Trưởng nhóm", Department = dev };
            var mBinh = new Mentor { FullName = "Trần Thị Bình", Email = "binh@congty.vn", Phone = "0987654321", Position = "Kỹ sư kiểm thử", Department = qa };
            var mCuong = new Mentor { FullName = "Lê Minh Cường", Email = "cuong@congty.vn", Phone = "0901122334", Position = "Designer", Department = design };
            db.Mentors.AddRange(mAn, mBinh, mCuong);

            var i1 = NewIntern("TTS001", "Phạm Thu Hà", "ha.pt@sv.edu.vn", "Đại học Phenikaa", "Công nghệ thông tin", dev, mAn, today.AddMonths(-2), today.AddMonths(1), InternStatus.Active);
            var i2 = NewIntern("TTS002", "Hoàng Đức Minh", "minh.hd@sv.edu.vn", "Đại học Bách khoa Hà Nội", "Khoa học máy tính", dev, mAn, today.AddMonths(-1), today.AddMonths(2), InternStatus.Active);
            var i3 = NewIntern("TTS003", "Vũ Lan Anh", "anh.vl@sv.edu.vn", "Đại học Phenikaa", "Hệ thống thông tin", qa, mBinh, today.AddMonths(-3), today.AddDays(-5), InternStatus.Completed);
            var i4 = NewIntern("TTS004", "Đỗ Quang Huy", "huy.dq@sv.edu.vn", "Đại học Công nghệ", "Kỹ thuật phần mềm", qa, mBinh, today.AddDays(10), today.AddMonths(3), InternStatus.Pending);
            var i5 = NewIntern("TTS005", "Ngô Mai Linh", "linh.nm@sv.edu.vn", "Đại học Kiến trúc", "Thiết kế đồ họa", design, mCuong, today.AddMonths(-1), today.AddMonths(2), InternStatus.Active);
            var i6 = NewIntern("TTS006", "Bùi Tiến Dũng", "dung.bt@sv.edu.vn", "Đại học Phenikaa", "Công nghệ thông tin", dev, null, today.AddMonths(-2), today.AddMonths(1), InternStatus.Terminated);
            db.Interns.AddRange(i1, i2, i3, i4, i5, i6);

            db.Users.Add(new User { Username = "admin", PasswordHash = PasswordHasher.Hash("admin123"), Role = UserRole.Admin });
            foreach (Mentor m in new[] { mAn, mBinh, mCuong })
                db.Users.Add(new User { Username = m.Email, PasswordHash = PasswordHasher.Hash(DefaultPassword), Role = UserRole.Mentor, Mentor = m });
            foreach (Intern i in new[] { i1, i2, i3, i4, i5, i6 })
                db.Users.Add(new User { Username = i.Code, PasswordHash = PasswordHasher.Hash(DefaultPassword), Role = UserRole.Intern, Intern = i });

            db.Tasks.AddRange(
                NewTask(i1, "Tìm hiểu quy trình Git của công ty", today.AddDays(-50), today.AddDays(-45), TaskItemStatus.Done),
                NewTask(i1, "Viết API đăng nhập", today.AddDays(-10), today.AddDays(-2), TaskItemStatus.InProgress),
                NewTask(i1, "Viết unit test cho module đăng nhập", today.AddDays(-1), today.AddDays(7), TaskItemStatus.Todo),
                NewTask(i2, "Cài đặt môi trường phát triển", today.AddDays(-25), today.AddDays(-20), TaskItemStatus.Done),
                NewTask(i2, "Làm màn hình danh sách sản phẩm", today.AddDays(-5), today.AddDays(5), TaskItemStatus.InProgress),
                NewTask(i3, "Viết test case cho chức năng thanh toán", today.AddDays(-60), today.AddDays(-40), TaskItemStatus.Done),
                NewTask(i5, "Thiết kế logo sự kiện", today.AddDays(-20), today.AddDays(-3), TaskItemStatus.Todo),
                NewTask(i5, "Vẽ wireframe trang chủ", today.AddDays(-2), today.AddDays(10), TaskItemStatus.InProgress));

            db.Evaluations.AddRange(
                NewEvaluation(i1, mAn, today.AddDays(-30), 9, 8, 8.5, "Chăm chỉ, tiếp thu nhanh"),
                NewEvaluation(i2, mAn, today.AddDays(-7), 8, 7, 7.5, "Cần chủ động hỏi khi gặp khó"),
                NewEvaluation(i3, mBinh, today.AddDays(-6), 9.5, 9, 9, "Hoàn thành xuất sắc kỳ thực tập"),
                NewEvaluation(i5, mCuong, today.AddDays(-10), 7, 6.5, 8, "Ý tưởng tốt, cần đúng hạn hơn"));

            db.SaveChanges();
        }

        private static Intern NewIntern(string code, string name, string email, string university, string major,
            Department department, Mentor mentor, DateTime start, DateTime end, InternStatus status)
        {
            return new Intern
            {
                Code = code,
                FullName = name,
                Email = email,
                University = university,
                Major = major,
                Department = department,
                Mentor = mentor,
                StartDate = start,
                EndDate = end,
                Status = status
            };
        }

        private static TaskItem NewTask(Intern intern, string title, DateTime assigned, DateTime due, TaskItemStatus status)
        {
            return new TaskItem { Intern = intern, Title = title, AssignedDate = assigned, DueDate = due, Status = status };
        }

        private static Evaluation NewEvaluation(Intern intern, Mentor mentor, DateTime date,
            double attitude, double skill, double teamwork, string comment)
        {
            return new Evaluation
            {
                Intern = intern,
                Mentor = mentor,
                EvaluationDate = date,
                AttitudeScore = attitude,
                SkillScore = skill,
                TeamworkScore = teamwork,
                Comment = comment
            };
        }
    }
}
