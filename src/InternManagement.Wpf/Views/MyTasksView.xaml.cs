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

        private void LoadData()
        {
            if (!CurrentInternId.HasValue)
                return;

            int internId = CurrentInternId.Value;

            List<TaskItem> tasks = AppServices.Tasks.GetTasks(internId: internId);
            gridTasks.ItemsSource = tasks;

            int total = tasks.Count;
            int done = tasks.Count(t => t.Status == TaskItemStatus.Done);
            pbProgress.Maximum = total > 0 ? total : 1;
            pbProgress.Value = done;
            lblProgressText.Text = string.Format("{0}/{1} công việc hoàn thành", done, total);

            List<Evaluation> evals = AppServices.Evaluations.GetEvaluations(internId: internId);
            gridEvaluations.ItemsSource = evals;

            UpdateButtons();
        }

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
            UpdateSelectedTaskStatus(TaskItemStatus.InProgress);
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedTaskStatus(TaskItemStatus.Done);
        }

        private void UpdateSelectedTaskStatus(TaskItemStatus status)
        {
            TaskItem selected = SelectedTask;
            if (selected == null || !CurrentInternId.HasValue)
                return;

            try
            {
                AppServices.Tasks.UpdateStatus(selected.Id, status, CurrentInternId.Value);
                LoadData();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể cập nhật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
