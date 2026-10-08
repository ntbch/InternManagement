using System.Windows;
using InternManagement.Core.Exceptions;

namespace InternManagement.Wpf.Views
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
            txtOld.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppServices.Auth.ChangePassword(Session.CurrentUser.Id,
                    txtOld.Password, txtNew.Password, txtConfirm.Password);
                MessageBox.Show("Đổi mật khẩu thành công.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
