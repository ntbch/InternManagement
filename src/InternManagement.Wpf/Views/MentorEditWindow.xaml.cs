using System.Collections.Generic;
using System.Windows;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    public partial class MentorEditWindow : Window
    {
        private readonly Mentor _mentor;
        private readonly bool _isNew;

        public MentorEditWindow(Mentor mentor)
        {
            InitializeComponent();
            _isNew = mentor == null;
            _mentor = mentor ?? new Mentor();
            Title = _isNew ? "Thêm mentor" : "Sửa mentor";

            List<Department> departments = AppServices.Departments.GetAll();
            cboDepartment.ItemsSource = departments;

            if (_isNew && departments.Count > 0)
                _mentor.DepartmentId = departments[0].Id;

            txtAccountNote.Visibility = _isNew ? Visibility.Visible : Visibility.Collapsed;

            DataContext = _mentor;
            txtFullName.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isNew)
                    AppServices.Mentors.Add(_mentor);
                else
                    AppServices.Mentors.Update(_mentor);

                DialogResult = true;
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
