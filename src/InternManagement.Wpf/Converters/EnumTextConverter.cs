using System;
using System.Globalization;
using System.Windows.Data;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Converters
{
    /// <summary>Hiển thị enum (trạng thái, vai trò) bằng chữ tiếng Việt trong Binding.</summary>
    public class EnumTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InternStatus)
                return StatusText.Of((InternStatus)value);
            if (value is TaskItemStatus)
                return StatusText.Of((TaskItemStatus)value);
            if (value is UserRole)
                return StatusText.Of((UserRole)value);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
