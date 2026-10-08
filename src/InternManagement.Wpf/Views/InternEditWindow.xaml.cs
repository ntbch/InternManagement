using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    /// <summary>
    /// Cửa sổ Thêm/Sửa Thực tập sinh:
    /// - DataContext gán đối tượng Intern, hỗ trợ Binding hai chiều (TwoWay).
    /// - Khi thêm mới: tự động sinh mã TTS kế tiếp, đặt ngày mặc định và trạng thái Chờ bắt đầu.
    /// - Danh sách mentor lọc theo phòng ban được chọn; có tùy chọn "(Chưa phân công)".
    /// </summary>
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

            // Nạp danh sách trạng thái thực tập vào ComboBox
            cboStatus.ItemsSource = Enum.GetValues(typeof(InternStatus));

            // Giá trị mặc định khi tạo mới thực tập sinh
            if (_isNew)
            {
                _intern.Code = AppServices.Interns.GenerateNextCode();
                _intern.StartDate = DateTime.Today;
                _intern.EndDate = DateTime.Today.AddMonths(3);
                _intern.Status = InternStatus.Pending;
                _intern.MentorId = null;
            }

            // Nạp danh sách phòng ban
            List<Department> departments = AppServices.Departments.GetAll();
            cboDepartment.ItemsSource = departments;

            if (_isNew && departments.Count > 0 && _intern.DepartmentId == 0)
            {
                _intern.DepartmentId = departments[0].Id;
            }

            // Nạp danh sách mentor thuộc phòng ban hiện tại
            LoadMentorsForDepartment(_intern.DepartmentId, _intern.MentorId);

            // Ghi chú thông tin tài khoản mặc định chỉ hiển thị khi thêm mới
            txtAccountNote.Visibility = _isNew ? Visibility.Visible : Visibility.Collapsed;

            DataContext = _intern;
            _isInitializing = false;
            txtCode.Focus();
        }

        private void LoadMentorsForDepartment(int departmentId, int? selectedMentorId)
        {
            var mentorList = new List<Mentor>();
            // Mục placeholder cho trường hợp thực tập sinh chưa được phân công mentor
            mentorList.Add(new Mentor { Id = 0, FullName = "(Chưa phân công)" });

            if (departmentId > 0)
            {
                mentorList.AddRange(AppServices.Mentors.GetAll(null, departmentId));
            }

            cboMentor.ItemsSource = mentorList;

            // Kiểm tra xem mentor cần chọn có nằm trong danh sách hay không
            int targetId = (selectedMentorId.HasValue && selectedMentorId.Value > 0) ? selectedMentorId.Value : 0;
            bool exists = false;
            foreach (Mentor m in mentorList)
            {
                if (m.Id == targetId)
                {
                    exists = true;
                    break;
                }
            }

            // Nếu mentor không thuộc phòng ban mới, đặt về placeholder "(Chưa phân công)" (Id = 0)
            cboMentor.SelectedValue = exists ? targetId : 0;
        }

        private void Department_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;

            int deptId = cboDepartment.SelectedValue is int ? (int)cboDepartment.SelectedValue : 0;
            int currentMentorId = cboMentor.SelectedValue is int ? (int)cboMentor.SelectedValue : 0;

            // Tải lại danh sách mentor theo phòng ban vừa chọn và chọn placeholder nếu mentor cũ không thuộc phòng ban này
            LoadMentorsForDepartment(deptId, currentMentorId > 0 ? (int?)currentMentorId : null);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int selectedDeptId = cboDepartment.SelectedValue is int ? (int)cboDepartment.SelectedValue : 0;
            _intern.DepartmentId = selectedDeptId;

            int selectedMentorId = cboMentor.SelectedValue is int ? (int)cboMentor.SelectedValue : 0;
            // Giá trị 0 là mục placeholder "(Chưa phân công)" - cần gán null trước khi lưu vào CSDL
            if (selectedMentorId == 0)
            {
                _intern.MentorId = null;
            }
            else
            {
                _intern.MentorId = selectedMentorId;
            }

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
