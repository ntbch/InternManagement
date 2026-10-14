using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
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
                    Header = dept.Name + " (" + dept.Interns.Count + " TTS)",
                    Tag = dept,
                    IsExpanded = true
                };

                foreach (Mentor mentor in dept.Mentors)
                {
                    var mentorNode = new TreeViewItem
                    {
                        Header = mentor.FullName,
                        Tag = mentor
                    };

                    foreach (Intern intern in mentor.Interns)
                    {
                        mentorNode.Items.Add(new TreeViewItem
                        {
                            Header = intern.Code + " - " + intern.FullName,
                            Tag = intern
                        });
                    }

                    deptNode.Items.Add(mentorNode);
                }

                var unassignedInterns = dept.Interns.Where(i => i.MentorId == null).ToList();
                if (unassignedInterns.Count > 0)
                {
                    var unassignedNode = new TreeViewItem
                    {
                        Header = "Chưa có mentor (" + unassignedInterns.Count + ")",
                        Tag = "UnassignedGroup"
                    };

                    foreach (Intern intern in unassignedInterns)
                    {
                        unassignedNode.Items.Add(new TreeViewItem
                        {
                            Header = intern.Code + " - " + intern.FullName,
                            Tag = intern
                        });
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

            txtLine1.Text = dept.Name;
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
