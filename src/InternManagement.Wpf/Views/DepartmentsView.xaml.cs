using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    /// <summary>
    /// Mẫu chung cho các màn hình danh sách:
    /// LoadData() lấy dữ liệu từ service gán vào DataGrid; Thêm/Sửa mở cửa sổ con; Xóa hỏi xác nhận.
    /// </summary>
    public partial class DepartmentsView : UserControl
    {
        public DepartmentsView()
        {
            InitializeComponent();
            LoadData();
        }

        private Department Selected
        {
            get { return grid.SelectedItem as Department; }
        }

        private void LoadData()
        {
            grid.ItemsSource = AppServices.Departments.GetAll();
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

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DepartmentEditWindow(null) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
                LoadData();
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
            var dialog = new DepartmentEditWindow(Selected) { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
            LoadData(); // tải lại kể cả khi hủy, để bỏ các thay đổi chưa lưu trên dòng đang sửa
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Department department = Selected;
            if (department == null)
                return;

            MessageBoxResult answer = MessageBox.Show(
                "Xóa phòng ban \"" + department.Name + "\"?", "Xác nhận xóa",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                AppServices.Departments.Delete(department.Id);
                LoadData();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
