using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    /// <summary>
    /// Màn hình Giao việc (dành cho Admin và Mentor).
    /// Hỗ trợ lọc theo thực tập sinh, trạng thái, công việc quá hạn; thêm/sửa/xóa công việc.
    /// </summary>
    public partial class TasksView : UserControl
    {
        private List<Intern> _interns;
        private bool _isLoaded;

        public TasksView()
        {
            InitializeComponent();
            InitFilters();
            _isLoaded = true;
            LoadData();
        }

        private TaskItem Selected
        {
            get { return grid.SelectedItem as TaskItem; }
        }

        /// <summary>
        /// Khởi tạo dữ liệu cho các ComboBox bộ lọc:
        /// - Thực tập sinh: Admin thấy tất cả, Mentor chỉ thấy TTS do mình hướng dẫn.
        /// - Trạng thái: lấy từ enum TaskItemStatus kết hợp mục tất cả.
        /// </summary>
        private void InitFilters()
        {
            _interns = Session.IsMentor
                ? AppServices.Interns.Search(mentorId: Session.MentorId)
                : AppServices.Interns.Search();

            var internItems = new List<Intern>();
            internItems.Add(new Intern { Id = 0, Code = "", FullName = "-- Tất cả thực tập sinh --" });
            internItems.AddRange(_interns);
            cboInternFilter.ItemsSource = internItems;
            cboInternFilter.SelectedIndex = 0;

            var statusItems = new List<object>();
            statusItems.Add("-- Tất cả trạng thái --");
            foreach (TaskItemStatus status in Enum.GetValues(typeof(TaskItemStatus)))
            {
                statusItems.Add(status);
            }
            cboStatusFilter.ItemsSource = statusItems;
            cboStatusFilter.SelectedIndex = 0;
        }

        /// <summary>
        /// Nạp danh sách công việc từ service theo các điều kiện lọc đang chọn.
        /// </summary>
        private void LoadData()
        {
            if (!_isLoaded)
                return;

            int? internId = null;
            if (cboInternFilter.SelectedItem is Intern intern && intern.Id > 0)
                internId = intern.Id;

            int? mentorId = Session.IsMentor ? Session.MentorId : null;

            TaskItemStatus? status = null;
            if (cboStatusFilter.SelectedItem is TaskItemStatus s)
                status = s;

            List<TaskItem> tasks = AppServices.Tasks.GetTasks(internId, mentorId, status);

            // IsOverdue là thuộc tính tính toán, không lưu trong CSDL nên lọc trên bộ nhớ bằng LINQ
            // Đây là minh họa điều khiển CheckBox (mục 3.6.4 đề cương môn học)
            if (chkOverdue != null && chkOverdue.IsChecked == true)
            {
                tasks = tasks.Where(t => t.IsOverdue).ToList();
            }

            grid.ItemsSource = tasks;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            btnEdit.IsEnabled = Selected != null;
            btnDelete.IsEnabled = Selected != null;
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded)
                return;
            LoadData();
        }

        private void Overdue_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
                return;
            LoadData();
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TaskEditWindow(null, _interns) { Owner = Window.GetWindow(this) };
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

            var dialog = new TaskEditWindow(Selected, _interns) { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
            LoadData(); // tải lại kể cả khi hủy, để bỏ các thay đổi chưa lưu trên dòng đang sửa
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            TaskItem task = Selected;
            if (task == null)
                return;

            MessageBoxResult answer = MessageBox.Show(
                "Xóa công việc \"" + task.Title + "\"?", "Xác nhận xóa",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                AppServices.Tasks.Delete(task.Id);
                LoadData();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
