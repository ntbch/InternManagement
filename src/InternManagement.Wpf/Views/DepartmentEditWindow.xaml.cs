using System.Windows;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    public partial class DepartmentEditWindow : Window
    {
        private readonly Department _department;
        private readonly bool _isNew;

        public DepartmentEditWindow(Department department)
        {
            InitializeComponent();
            _isNew = department == null;
            _department = department ?? new Department();
            Title = _isNew ? "Thêm phòng ban" : "Sửa phòng ban";
            DataContext = _department;
            txtName.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isNew)
                    AppServices.Departments.Add(_department);
                else
                    AppServices.Departments.Update(_department);
                DialogResult = true;
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
