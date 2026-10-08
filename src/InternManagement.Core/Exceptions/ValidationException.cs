using System;

namespace InternManagement.Core.Exceptions
{
    /// <summary>
    /// Lỗi nghiệp vụ / dữ liệu nhập không hợp lệ. Giao diện bắt lỗi này và hiện MessageBox.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}
