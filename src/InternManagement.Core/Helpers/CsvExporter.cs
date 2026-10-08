using System.Collections.Generic;
using System.IO;
using System.Text;
using InternManagement.Core.Models;

namespace InternManagement.Core.Helpers
{
    /// <summary>Xuất danh sách thực tập sinh ra file CSV (mở được bằng Excel).</summary>
    public static class CsvExporter
    {
        public static void ExportInterns(IEnumerable<Intern> interns, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Mã,Họ tên,Email,Điện thoại,Trường,Chuyên ngành,Phòng ban,Mentor,Bắt đầu,Kết thúc,Trạng thái");

            foreach (Intern i in interns)
            {
                string[] cells =
                {
                    i.Code,
                    i.FullName,
                    i.Email,
                    i.Phone,
                    i.University,
                    i.Major,
                    i.Department != null ? i.Department.Name : "",
                    i.Mentor != null ? i.Mentor.FullName : "",
                    i.StartDate.ToString("dd/MM/yyyy"),
                    i.EndDate.ToString("dd/MM/yyyy"),
                    StatusText.Of(i.Status)
                };

                for (int c = 0; c < cells.Length; c++)
                {
                    if (c > 0) sb.Append(',');
                    sb.Append(Escape(cells[c]));
                }
                sb.AppendLine();
            }

            // Encoding.UTF8 ghi kèm BOM để Excel hiển thị đúng tiếng Việt
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>Ô chứa dấu phẩy, nháy kép hoặc xuống dòng phải bọc trong nháy kép.</summary>
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
