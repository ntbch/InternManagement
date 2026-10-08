using System;
using System.Linq;
using InternManagement.Core.Data;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Core.Services
{
    /// <summary>Đăng nhập và đổi mật khẩu.</summary>
    public class AuthService
    {
        private readonly Func<AppDbContext> _createDb;

        public AuthService(Func<AppDbContext> createDb)
        {
            _createDb = createDb;
        }

        /// <summary>Trả về tài khoản nếu đúng tên đăng nhập và mật khẩu; sai thì ném ValidationException.</summary>
        public User Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                throw new ValidationException("Vui lòng nhập tên đăng nhập và mật khẩu.");

            string name = username.Trim().ToLowerInvariant();
            using (AppDbContext db = _createDb())
            {
                User user = db.Users
                    .Include(u => u.Mentor)
                    .Include(u => u.Intern)
                    .FirstOrDefault(u => u.Username.ToLower() == name);

                // Không nói rõ sai tên hay sai mật khẩu để tránh lộ thông tin tài khoản
                if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                    throw new ValidationException("Sai tên đăng nhập hoặc mật khẩu.");

                return user;
            }
        }

        public void ChangePassword(int userId, string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                throw new ValidationException("Mật khẩu mới phải có ít nhất 6 ký tự.");
            if (newPassword != confirmPassword)
                throw new ValidationException("Xác nhận mật khẩu không khớp.");

            using (AppDbContext db = _createDb())
            {
                User user = db.Users.Find(userId);
                if (user == null)
                    throw new ValidationException("Không tìm thấy tài khoản.");
                if (!PasswordHasher.Verify(oldPassword, user.PasswordHash))
                    throw new ValidationException("Mật khẩu hiện tại không đúng.");

                user.PasswordHash = PasswordHasher.Hash(newPassword);
                db.SaveChanges();
            }
        }
    }
}
