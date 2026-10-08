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
    /// Màn hình Đánh giá thực tập sinh:
    /// - Admin xem tất cả đánh giá ở chế độ chỉ đọc (ẩn nút Thêm/Sửa/Xóa, hiển thị ghi chú).
    /// - Mentor xem và quản lý các phiếu đánh giá do chính mình tạo (có nút Thêm/Sửa/Xóa).
    /// </summary>
    public partial class EvaluationsView : UserControl
    {
        public EvaluationsView()
        {
            InitializeComponent();
            ApplyRolePermissions();
            LoadData();
        }

        private Evaluation Selected
        {
            get { return grid.SelectedItem as Evaluation; }
        }

        private void ApplyRolePermissions()
        {
            if (Session.IsMentor)
            {
                actionBar.Visibility = Visibility.Visible;
                txtAdminNote.Visibility = Visibility.Collapsed;
            }
            else
            {
                actionBar.Visibility = Visibility.Collapsed;
                txtAdminNote.Visibility = Visibility.Visible;
            }
        }

        private void LoadData()
        {
            if (Session.IsMentor)
            {
                grid.ItemsSource = AppServices.Evaluations.GetEvaluations(mentorId: Session.MentorId);
            }
            else
            {
                grid.ItemsSource = AppServices.Evaluations.GetEvaluations();
            }
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (Session.IsMentor)
            {
                btnEdit.IsEnabled = Selected != null;
                btnDelete.IsEnabled = Selected != null;
            }
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsMentor)
                return;

            List<Intern> interns = AppServices.Interns.Search(mentorId: Session.MentorId);
            if (interns == null || interns.Count == 0)
            {
                MessageBox.Show("Bạn chưa được phân công thực tập sinh nào.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new EvaluationEditWindow(null, interns) { Owner = Window.GetWindow(this) };
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
            if (!Session.IsMentor || Selected == null)
                return;

            List<Intern> interns = AppServices.Interns.Search(mentorId: Session.MentorId);
            // Đảm bảo thực tập sinh của phiếu đánh giá này luôn có trong danh sách chọn
            if (Selected.Intern != null && interns != null && !interns.Any(i => i.Id == Selected.InternId))
            {
                interns.Insert(0, Selected.Intern);
            }

            var dialog = new EvaluationEditWindow(Selected, interns) { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
            LoadData(); // tải lại kể cả khi hủy, để bỏ các thay đổi chưa lưu trên dòng đang sửa
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsMentor)
                return;

            Evaluation evaluation = Selected;
            if (evaluation == null)
                return;

            string internName = evaluation.Intern != null ? evaluation.Intern.FullName : "thực tập sinh";
            MessageBoxResult answer = MessageBox.Show(
                "Xóa đánh giá của thực tập sinh \"" + internName + "\"?", "Xác nhận xóa",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                AppServices.Evaluations.Delete(evaluation.Id);
                LoadData();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
