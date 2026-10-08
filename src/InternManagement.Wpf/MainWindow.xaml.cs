using System.Windows;
using System.Windows.Controls;
using InternManagement.Core.Helpers;
using InternManagement.Wpf.Views;

namespace InternManagement.Wpf
{
    /// <summary>
    /// Màn hình chính: Menu + ToolBar ở trên, nội dung ở giữa, StatusBar ở dưới.
    /// Mỗi chức năng là một UserControl được đặt vào pageHost.
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _loggingOut;

        public MainWindow()
        {
            InitializeComponent();
            ApplyPermissions();

            txtUser.Text = "Xin chào, " + Session.CurrentUser.DisplayName
                           + " (" + StatusText.Of(Session.CurrentUser.Role) + ")";

            // Trang mở đầu tùy theo vai trò
            if (Session.IsAdmin)
                Navigate(new DashboardView(), "Tổng quan");
            else if (Session.IsMentor)
                Navigate(new InternsView(), "Thực tập sinh của tôi");
            else
                Navigate(new MyTasksView(), "Thực tập của tôi");
        }

        /// <summary>Ẩn các chức năng không thuộc quyền của tài khoản đang đăng nhập.</summary>
        private void ApplyPermissions()
        {
            bool admin = Session.IsAdmin;
            bool mentor = Session.IsMentor;
            bool intern = Session.IsIntern;

            SetVisible(admin, menuDashboard, menuOrgTree, sepManage, menuDepartments, menuMentors, tbDashboard);
            SetVisible(admin || mentor, menuManage, menuInterns, tbInterns, menuTasks, tbTasks, menuEvaluations, tbEvaluations);
            SetVisible(intern, menuMyTasks, tbMyTasks);

            if (mentor)
                menuInterns.Header = "Thực tập sinh của tôi";
        }

        private static void SetVisible(bool visible, params FrameworkElement[] elements)
        {
            foreach (FrameworkElement element in elements)
                element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Navigate(UserControl page, string title)
        {
            pageHost.Content = page;
            txtPage.Text = title;
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new DashboardView(), "Tổng quan");
        }

        private void OrgTree_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new OrgTreeView(), "Sơ đồ tổ chức");
        }

        private void Departments_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new DepartmentsView(), "Phòng ban");
        }

        private void Mentors_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new MentorsView(), "Mentor");
        }

        private void Interns_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new InternsView(), Session.IsMentor ? "Thực tập sinh của tôi" : "Thực tập sinh");
        }

        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new TasksView(), "Giao việc");
        }

        private void Evaluations_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new EvaluationsView(), "Đánh giá");
        }

        private void MyTasks_Click(object sender, RoutedEventArgs e)
        {
            Navigate(new MyTasksView(), "Thực tập của tôi");
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            new ChangePasswordWindow { Owner = this }.ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Hệ thống quản lý thực tập sinh\nBài tập lớn học phần Công nghệ .NET (CSE703009)\nĐại học Phenikaa",
                "Giới thiệu", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn muốn đăng xuất?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _loggingOut = true;
            Close();
            ((App)Application.Current).ShowLogin();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>Đóng cửa sổ chính (không phải đăng xuất) thì thoát ứng dụng.</summary>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (!_loggingOut)
                Application.Current.Shutdown();
        }
    }
}
