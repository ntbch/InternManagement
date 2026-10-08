using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    /// <summary>
    /// Sơ đồ tổ chức phân cấp: Phòng ban -> Mentor -> Thực tập sinh.
    /// Minh họa sử dụng TreeView trong WPF và tính ĐA HÌNH (Polymorphism) trong OOP:
    /// Gọi person.GetDisplayInfo() trên biến kiểu cha Person,
    /// phương thức ảo sẽ tự động thực thi phiên bản phù hợp của Mentor hoặc Intern tại runtime.
    /// </summary>
    public partial class OrgTreeView : UserControl
    {
        public OrgTreeView()
        {
            InitializeComponent();
            BuildTree();
        }

        private void BuildTree()
        {
            treeOrg.Items.Clear();
            List<Department> departments = AppServices.Departments.GetAll();

            foreach (Department dept in departments)
            {
                var deptNode = new TreeViewItem
                {
                    Header = "🏢 " + dept.Name + " (" + dept.Interns.Count + " TTS)",
                    Tag = dept,
                    IsExpanded = true // Tự động mở rộng các nút phòng ban
                };

                // Nạp các mentor thuộc phòng ban
                foreach (Mentor mentor in dept.Mentors)
                {
                    var mentorNode = new TreeViewItem
                    {
                        Header = "👤 " + mentor.FullName,
                        Tag = mentor
                    };

                    // Nạp các thực tập sinh được mentor này hướng dẫn
                    foreach (Intern intern in mentor.Interns)
                    {
                        var internNode = new TreeViewItem
                        {
                            Header = "🎓 " + intern.Code + " - " + intern.FullName,
                            Tag = intern
                        };
                        mentorNode.Items.Add(internNode);
                    }

                    deptNode.Items.Add(mentorNode);
                }

                // Nhóm các thực tập sinh thuộc phòng ban nhưng chưa được phân công mentor
                var unassignedInterns = new List<Intern>();
                foreach (Intern i in dept.Interns)
                {
                    if (i.MentorId == null)
                    {
                        unassignedInterns.Add(i);
                    }
                }

                if (unassignedInterns.Count > 0)
                {
                    var unassignedNode = new TreeViewItem
                    {
                        Header = "⚠️ Chưa có mentor (" + unassignedInterns.Count + ")",
                        Tag = "UnassignedGroup"
                    };

                    foreach (Intern intern in unassignedInterns)
                    {
                        var internNode = new TreeViewItem
                        {
                            Header = "🎓 " + intern.Code + " - " + intern.FullName,
                            Tag = intern
                        };
                        unassignedNode.Items.Add(internNode);
                    }

                    deptNode.Items.Add(unassignedNode);
                }

                treeOrg.Items.Add(deptNode);
            }
        }

        private void TreeOrg_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = treeOrg.SelectedItem as TreeViewItem;
            if (selectedItem == null || selectedItem.Tag == null)
            {
                ShowPlaceholder();
                return;
            }

            object tag = selectedItem.Tag;

            // Kiểm tra kiểu dữ liệu của Tag:
            // Nếu là Person (Mentor hoặc Intern): gọi phương thức ảo GetDisplayInfo() (tính ĐA HÌNH)
            if (tag is Person)
            {
                Person person = (Person)tag;
                ShowPersonDetail(person);
            }
            else if (tag is Department)
            {
                Department dept = (Department)tag;
                ShowDepartmentDetail(dept);
            }
            else
            {
                txtDetailTitle.Text = "Nhóm thực tập sinh chưa có mentor";
                txtPlaceholder.Visibility = Visibility.Collapsed;
                panelContent.Visibility = Visibility.Visible;
                txtLine1.Text = selectedItem.Header.ToString();
                txtLine2.Text = "Bao gồm các thực tập sinh thuộc phòng ban nhưng chưa được phân công mentor hướng dẫn.";
                txtLine3.Text = "";
                txtLine4.Text = "";
                txtLine5.Text = "";
                txtLine6.Text = "";
            }
        }

        private void ShowPersonDetail(Person person)
        {
            txtPlaceholder.Visibility = Visibility.Collapsed;
            panelContent.Visibility = Visibility.Visible;

            // Gọi phương thức ảo (virtual) GetDisplayInfo() - minh họa tính ĐA HÌNH trong OOP:
            // Biến person có kiểu khai báo là Person, nhưng tại thời điểm chạy (runtime),
            // chương trình sẽ tự động gọi GetDisplayInfo() của lớp con tương ứng (Mentor hoặc Intern).
            txtLine1.Text = person.GetDisplayInfo();
            txtLine2.Text = "Điện thoại: " + (string.IsNullOrEmpty(person.Phone) ? "(Chưa cập nhật)" : person.Phone);

            if (person is Mentor)
            {
                Mentor mentor = (Mentor)person;
                txtDetailTitle.Text = "Thông tin Mentor";
                txtLine3.Text = "Chức vụ: " + (string.IsNullOrEmpty(mentor.Position) ? "(Chưa cập nhật)" : mentor.Position);
                txtLine4.Text = "Phòng ban: " + (mentor.Department != null ? mentor.Department.Name : "");
                txtLine5.Text = "Số thực tập sinh đang hướng dẫn: " + mentor.Interns.Count;
                txtLine6.Text = "";
            }
            else if (person is Intern)
            {
                Intern intern = (Intern)person;
                txtDetailTitle.Text = "Thông tin Thực tập sinh";
                txtLine3.Text = "Trường: " + (string.IsNullOrEmpty(intern.University) ? "(Chưa cập nhật)" : intern.University)
                              + " | Chuyên ngành: " + (string.IsNullOrEmpty(intern.Major) ? "(Chưa cập nhật)" : intern.Major);
                txtLine4.Text = "Thời gian: " + intern.StartDate.ToString("dd/MM/yyyy") + " - " + intern.EndDate.ToString("dd/MM/yyyy");
                txtLine5.Text = "Trạng thái: " + StatusText.Of(intern.Status);
                txtLine6.Text = "Mentor: " + (intern.Mentor != null ? intern.Mentor.FullName : "(Chưa phân công)");
            }
        }

        private void ShowDepartmentDetail(Department dept)
        {
            txtDetailTitle.Text = "Thông tin Phòng ban";
            txtPlaceholder.Visibility = Visibility.Collapsed;
            panelContent.Visibility = Visibility.Visible;

            txtLine1.Text = "🏢 " + dept.Name;
            txtLine2.Text = "Mô tả: " + (string.IsNullOrEmpty(dept.Description) ? "(Không có mô tả)" : dept.Description);
            txtLine3.Text = "Số mentor: " + dept.Mentors.Count;
            txtLine4.Text = "Số thực tập sinh: " + dept.Interns.Count;
            txtLine5.Text = "";
            txtLine6.Text = "";
        }

        private void ShowPlaceholder()
        {
            txtDetailTitle.Text = "Thông tin chi tiết";
            txtPlaceholder.Visibility = Visibility.Visible;
            panelContent.Visibility = Visibility.Collapsed;
        }
    }
}
