using System.Text.RegularExpressions;
using InternManagement.Core.Exceptions;

namespace InternManagement.Core.Helpers
{
    /// <summary>Các hàm kiểm tra dữ liệu nhập dùng chung. Sai thì ném ValidationException.</summary>
    public static class Validator
    {
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private static readonly Regex PhoneRegex = new Regex(@"^0\d{9}$");

        public static string Required(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException(fieldName + " không được để trống.");
            return value.Trim();
        }

        public static string Email(string value)
        {
            string email = Required(value, "Email");
            if (!EmailRegex.IsMatch(email))
                throw new ValidationException("Email không đúng định dạng.");
            return email.ToLowerInvariant();
        }

        /// <summary>Số điện thoại không bắt buộc; nếu nhập thì phải gồm 10 chữ số, bắt đầu bằng 0.</summary>
        public static string OptionalPhone(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            string phone = value.Trim();
            if (!PhoneRegex.IsMatch(phone))
                throw new ValidationException("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.");
            return phone;
        }

        public static string Optional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static void Score(double value, string fieldName)
        {
            if (value < 0 || value > 10)
                throw new ValidationException(fieldName + " phải nằm trong khoảng 0 - 10.");
        }
    }
}
