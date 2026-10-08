using System.Windows;
using InternManagement.Core.Exceptions;

namespace InternManagement.Wpf
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Session.CurrentUser = AppServices.Auth.Login(txtUsername.Text, txtPassword.Password);
                DialogResult = true; // đóng cửa sổ, App sẽ mở MainWindow
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
