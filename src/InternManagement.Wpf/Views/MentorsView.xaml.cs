using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    /// <summary>
    /// Màn hình quản lý danh sách Mentor dành cho Quản trị viên:
    /// Cho phép tìm kiếm theo từ khóa (họ tên, email), lọc theo phòng ban, Thêm, Sửa, Xóa.
    /// </summary>
    public partial class MentorsView : UserControl
    {
        public MentorsView()
        {
            InitializeComponent();
            LoadDepartments();
            LoadData();
        }

        private Mentor Selected
        {
            get { return grid.SelectedItem as Mentor; }
        }

        private void LoadDepartments()
        {
            var list = new List<Department>();
            list.Add(new Department { Id = 0, Name = "-- Tất cả phòng ban --" });
            list.AddRange(AppServices.Departments.GetAll());
            cboDepartment.ItemsSource = list;
            cboDepartment.SelectedIndex = 0;
        }

        private void LoadData()
        {
            if (txtSearch == null || cboDepartment == null || grid == null)
                return;

            string keyword = txtSearch.Text;
            int selectedDeptId = cboDepartment.SelectedValue is int ? (int)cboDepartment.SelectedValue : 0;
            int? departmentId = selectedDeptId > 0 ? (int?)selectedDeptId : null;

            grid.ItemsSource = AppServices.Mentors.GetAll(keyword, departmentId);
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            btnEdit.IsEnabled = Selected != null;
            btnDelete.IsEnabled = Selected != null;
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void Department_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MentorEditWindow(null) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        /// <summary>Nhấp đúp vào một dòng để sửa (bỏ qua tiêu đề cột, thanh cuộn, vùng trống).</summary>
        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(grid, (DependencyObject)e.OriginalSource) is DataGridRow)
                Edit_Click(sender, e);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Selected == null)
                return;
            var dialog = new MentorEditWindow(Selected) { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
            LoadData(); // Tải lại kể cả khi hủy để bỏ các thay đổi chưa lưu
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Mentor mentor = Selected;
            if (mentor == null)
                return;

            MessageBoxResult answer = MessageBox.Show(
                "Xóa mentor \"" + mentor.FullName + "\"?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                AppServices.Mentors.Delete(mentor.Id);
                LoadData();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
