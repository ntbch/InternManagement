using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    public partial class InternEditWindow : Window
    {
        private readonly Intern _intern;
        private readonly bool _isNew;
        private bool _isInitializing = true;

        public InternEditWindow(Intern intern)
        {
            InitializeComponent();
            _isNew = intern == null;
            _intern = intern ?? new Intern();
            Title = _isNew ? "Thêm thực tập sinh" : "Sửa thực tập sinh";

            cboStatus.ItemsSource = Enum.GetValues(typeof(InternStatus));

            if (_isNew)
            {
                _intern.Code = AppServices.Interns.GenerateNextCode();
                _intern.StartDate = DateTime.Today;
                _intern.EndDate = DateTime.Today.AddMonths(3);
                _intern.Status = InternStatus.Pending;
            }

            List<Department> departments = AppServices.Departments.GetAll();
            cboDepartment.ItemsSource = departments;

            if (_isNew && departments.Count > 0)
                _intern.DepartmentId = departments[0].Id;

            LoadMentorsForDepartment(_intern.DepartmentId, _intern.MentorId);

            txtAccountNote.Visibility = _isNew ? Visibility.Visible : Visibility.Collapsed;

            DataContext = _intern;
            _isInitializing = false;
            txtCode.Focus();
        }

        private void LoadMentorsForDepartment(int departmentId, int? selectedMentorId)
        {
            var mentorList = new List<Mentor>();
            mentorList.Add(new Mentor { Id = 0, FullName = "(Chưa phân công)" });

            if (departmentId > 0)
            {
                mentorList.AddRange(AppServices.Mentors.GetAll(null, departmentId));
            }

            cboMentor.ItemsSource = mentorList;

            int targetId = (selectedMentorId.HasValue && selectedMentorId.Value > 0) ? selectedMentorId.Value : 0;
            cboMentor.SelectedValue = mentorList.Any(m => m.Id == targetId) ? targetId : 0;
        }

        private void Department_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;

            int deptId = cboDepartment.SelectedValue is int ? (int)cboDepartment.SelectedValue : 0;
            int currentMentorId = cboMentor.SelectedValue is int ? (int)cboMentor.SelectedValue : 0;

            LoadMentorsForDepartment(deptId, currentMentorId > 0 ? (int?)currentMentorId : null);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int selectedDeptId = cboDepartment.SelectedValue is int ? (int)cboDepartment.SelectedValue : 0;
            _intern.DepartmentId = selectedDeptId;

            int selectedMentorId = cboMentor.SelectedValue is int ? (int)cboMentor.SelectedValue : 0;
            _intern.MentorId = selectedMentorId > 0 ? (int?)selectedMentorId : null;

            try
            {
                if (_isNew)
                    AppServices.Interns.Add(_intern);
                else
                    AppServices.Interns.Update(_intern);

                DialogResult = true;
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
