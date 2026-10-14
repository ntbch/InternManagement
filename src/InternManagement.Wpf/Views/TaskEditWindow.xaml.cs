using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    public partial class TaskEditWindow : Window
    {
        private readonly TaskItem _task;
        private readonly bool _isNew;

        public TaskEditWindow(TaskItem task, List<Intern> interns)
        {
            InitializeComponent();
            _isNew = task == null;
            if (_isNew)
            {
                _task = new TaskItem
                {
                    AssignedDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(7),
                    Status = TaskItemStatus.Todo
                };
            }
            else
            {
                _task = task;
            }

            Title = _isNew ? "Giao việc mới" : "Sửa công việc";

            var internList = new List<Intern>(interns ?? new List<Intern>());
            if (!_isNew && _task.Intern != null && !internList.Any(i => i.Id == _task.InternId))
            {
                internList.Add(_task.Intern);
            }
            cboIntern.ItemsSource = internList;

            cboStatus.ItemsSource = Enum.GetValues(typeof(TaskItemStatus));

            DataContext = _task;

            txtTitle.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isNew)
                    AppServices.Tasks.Add(_task);
                else
                    AppServices.Tasks.Update(_task);

                DialogResult = true;
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
