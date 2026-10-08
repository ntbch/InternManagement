using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;
using Microsoft.Win32;

namespace InternManagement.Wpf.Views
{
    /// <summary>
    /// Màn hình danh sách Thực tập sinh:
    /// - Quản trị viên (Admin): xem tất cả, lọc đa tiêu chí, Thêm / Sửa / Xóa / Xuất CSV.
    /// - Người hướng dẫn (Mentor): chỉ xem thực tập sinh mình hướng dẫn, xem chi tiết qua đa hình GetDisplayInfo(), Xuất CSV.
    /// </summary>
    public partial class InternsView : UserControl
    {
        private bool _isInitializing = true;

        public InternsView()
        {
            InitializeComponent();
            InitializeView();
            _isInitializing = false;
            LoadData();
        }

        private Intern Selected
        {
            get { return grid.SelectedItem as Intern; }
        }

        private void InitializeView()
        {
            // Nạp danh sách trạng thái vào ComboBox (mục đầu tiên là chuỗi hiển thị tất cả)
            var statusList = new List<object>();
            statusList.Add("-- Tất cả trạng thái --");
            foreach (InternStatus s in Enum.GetValues(typeof(InternStatus)))
            {
                statusList.Add(s);
            }
            cboStatus.ItemsSource = statusList;
            cboStatus.SelectedIndex = 0;

            // Nạp danh sách phòng ban
            var deptList = new List<Department>();
            deptList.Add(new Department { Id = 0, Name = "-- Tất cả phòng ban --" });
            deptList.AddRange(AppServices.Departments.GetAll());
            cboDepartment.ItemsSource = deptList;
            cboDepartment.SelectedIndex = 0;

            // Phân quyền hiển thị theo vai trò người đăng nhập
            if (Session.IsMentor)
            {
                txtTitle.Text = "Thực tập sinh của tôi";
                btnAdd.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                lblMentor.Visibility = Visibility.Collapsed;
                cboMentor.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoadMentors();
            }
        }

        private void LoadMentors()
        {
            if (Session.IsMentor)
                return;

            int selectedDeptId = cboDepartment.SelectedValue is int ? (int)cboDepartment.SelectedValue : 0;
            int? deptId = selectedDeptId > 0 ? (int?)selectedDeptId : null;

            var mentorList = new List<Mentor>();
            mentorList.Add(new Mentor { Id = 0, FullName = "-- Tất cả mentor --" });
            mentorList.AddRange(AppServices.Mentors.GetAll(null, deptId));

            int currentSelectedId = cboMentor.SelectedValue is int ? (int)cboMentor.SelectedValue : 0;
            cboMentor.ItemsSource = mentorList;

            // Giữ lại mentor đang chọn nếu vẫn nằm trong danh sách mới
            bool found = false;
            foreach (Mentor m in mentorList)
            {
                if (m.Id == currentSelectedId)
                {
                    found = true;
                    break;
                }
            }
            cboMentor.SelectedValue = found ? currentSelectedId : 0;
        }

        private void LoadData()
        {
            if (_isInitializing || grid == null || cboStatus == null || cboDepartment == null)
                return;

            string keyword = txtSearch.Text;

            InternStatus? status = null;
            if (cboStatus.SelectedItem is InternStatus)
            {
                status = (InternStatus)cboStatus.SelectedItem;
            }

            int selectedDeptId = cboDepartment.SelectedValue is int ? (int)cboDepartment.SelectedValue : 0;
            int? departmentId = selectedDeptId > 0 ? (int?)selectedDeptId : null;

            int? mentorId;
            if (Session.IsMentor)
            {
                // Mentor chỉ được xem các thực tập sinh do chính mình hướng dẫn
                mentorId = Session.MentorId;
            }
            else
            {
                int selectedMentorId = cboMentor.SelectedValue is int ? (int)cboMentor.SelectedValue : 0;
                mentorId = selectedMentorId > 0 ? (int?)selectedMentorId : null;
            }

            grid.ItemsSource = AppServices.Interns.Search(keyword, status, departmentId, mentorId);
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (Session.IsMentor)
                return;

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

        private void Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private void Mentor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private void Department_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;

            LoadMentors();
            LoadData();
        }

        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Chỉ xử lý khi nhấp đúp vào một dòng (bỏ qua tiêu đề cột, thanh cuộn, vùng trống)
            if (!(ItemsControl.ContainerFromElement(grid, (DependencyObject)e.OriginalSource) is DataGridRow) || Selected == null)
                return;

            if (Session.IsMentor)
            {
                // Gọi phương thức ảo GetDisplayInfo() - minh họa tính ĐA HÌNH trong lập trình hướng đối tượng
                string info = Selected.GetDisplayInfo()
                    + "\nTrường: " + (string.IsNullOrEmpty(Selected.University) ? "Chưa cập nhật" : Selected.University)
                    + "\nChuyên ngành: " + (string.IsNullOrEmpty(Selected.Major) ? "Chưa cập nhật" : Selected.Major)
                    + "\nThời gian: " + Selected.StartDate.ToString("dd/MM/yyyy") + " - " + Selected.EndDate.ToString("dd/MM/yyyy")
                    + "\nTrạng thái: " + StatusText.Of(Selected.Status);

                MessageBox.Show(info, "Thông tin thực tập sinh", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Admin nhấp đúp để mở cửa sổ chỉnh sửa
                Edit_Click(sender, e);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InternEditWindow(null) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Selected == null)
                return;

            var dialog = new InternEditWindow(Selected) { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
            LoadData(); // Tải lại dữ liệu kể cả khi hủy để khôi phục trạng thái ban đầu
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Intern intern = Selected;
            if (intern == null)
                return;

            MessageBoxResult answer = MessageBox.Show(
                "Xóa thực tập sinh \"" + intern.FullName + "\" (" + intern.Code + ")?\n" +
                "Lưu ý: Mọi công việc, đánh giá và tài khoản đăng nhập của thực tập sinh này cũng sẽ bị xóa vĩnh viễn.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                AppServices.Interns.Delete(intern.Id);
                LoadData();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var list = grid.ItemsSource as IEnumerable<Intern>;
            if (list == null)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = "DanhSachThucTapSinh.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    CsvExporter.ExportInterns(list, dialog.FileName);
                    MessageBox.Show("Xuất danh sách thực tập sinh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi xuất file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Không có quyền ghi file: " + ex.Message, "Lỗi xuất file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
