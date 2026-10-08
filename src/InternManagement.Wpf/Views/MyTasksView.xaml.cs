using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    /// <summary>
    /// Màn hình dành cho thực tập sinh (Intern):
    /// Xem thông tin cá nhân, theo dõi tiến độ hoàn thành công việc,
    /// tự cập nhật trạng thái làm việc và xem kết quả đánh giá từ mentor.
    /// </summary>
    public partial class MyTasksView : UserControl
    {
        public MyTasksView()
        {
            InitializeComponent();
            if (Session.InternId.HasValue)
            {
                LoadProfile();
                LoadData();
            }
        }

        private int? CurrentInternId
        {
            get { return Session.InternId; }
        }

        private TaskItem SelectedTask
        {
            get { return gridTasks.SelectedItem as TaskItem; }
        }

        /// <summary>
        /// Nạp thông tin hồ sơ của thực tập sinh đang đăng nhập vào Card phía trên.
        /// </summary>
        private void LoadProfile()
        {
            if (!CurrentInternId.HasValue)
                return;

            Intern intern = AppServices.Interns.GetById(CurrentInternId.Value);
            if (intern == null)
                return;

            lblCode.Text = intern.Code ?? "";
            lblName.Text = intern.FullName ?? "";
            lblDepartment.Text = intern.Department != null ? intern.Department.Name : "Chưa phân công";
            lblMentor.Text = intern.Mentor != null ? intern.Mentor.FullName : "Chưa phân công";
            lblPeriod.Text = string.Format("{0:dd/MM/yyyy} – {1:dd/MM/yyyy}", intern.StartDate, intern.EndDate);
            lblStatus.Text = StatusText.Of(intern.Status);
        }

        /// <summary>
        /// Tải danh sách công việc, tính tiến độ hoàn thành và tải phiếu đánh giá.
        /// </summary>
        private void LoadData()
        {
            if (!CurrentInternId.HasValue)
                return;

            int internId = CurrentInternId.Value;

            // Lấy danh sách công việc được giao cho thực tập sinh này
            List<TaskItem> tasks = AppServices.Tasks.GetTasks(internId: internId);
            gridTasks.ItemsSource = tasks;

            // Tính tỷ lệ hoàn thành: số việc Done / tổng số việc
            int total = tasks.Count;
            int done = tasks.Count(t => t.Status == TaskItemStatus.Done);
            pbProgress.Maximum = total > 0 ? total : 1;
            pbProgress.Value = done;
            lblProgressText.Text = string.Format("{0}/{1} công việc hoàn thành", done, total);

            // Lấy danh sách phiếu đánh giá của thực tập sinh này
            List<Evaluation> evals = AppServices.Evaluations.GetEvaluations(internId: internId);
            gridEvaluations.ItemsSource = evals;

            UpdateButtons();
        }

        /// <summary>
        /// Chỉ bật nút khi đã chọn công việc và công việc chưa ở trạng thái đó.
        /// </summary>
        private void UpdateButtons()
        {
            TaskItem selected = SelectedTask;
            btnStart.IsEnabled = selected != null && selected.Status != TaskItemStatus.InProgress;
            btnComplete.IsEnabled = selected != null && selected.Status != TaskItemStatus.Done;
        }

        private void GridTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            TaskItem selected = SelectedTask;
            if (selected == null || !CurrentInternId.HasValue)
                return;

            try
            {
                AppServices.Tasks.UpdateStatus(selected.Id, TaskItemStatus.InProgress, CurrentInternId.Value);
                LoadData();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể cập nhật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            TaskItem selected = SelectedTask;
            if (selected == null || !CurrentInternId.HasValue)
                return;

            try
            {
                AppServices.Tasks.UpdateStatus(selected.Id, TaskItemStatus.Done, CurrentInternId.Value);
                LoadData();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể cập nhật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
