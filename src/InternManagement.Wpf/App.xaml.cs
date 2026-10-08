using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace InternManagement.Wpf
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Hiển thị ngày tháng theo định dạng Việt Nam (dd/MM/yyyy)
            var culture = new CultureInfo("vi-VN");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            // Lỗi không lường trước: báo cho người dùng thay vì để ứng dụng tắt đột ngột
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show("Đã xảy ra lỗi: " + args.Exception.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            try
            {
                AppServices.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khởi tạo cơ sở dữ liệu: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            ShowLogin();
        }

        /// <summary>Hiện cửa sổ đăng nhập; đăng nhập thành công thì mở màn hình chính, hủy thì thoát.</summary>
        public void ShowLogin()
        {
            Session.CurrentUser = null;
            var login = new LoginWindow();
            if (login.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            try
            {
                var main = new MainWindow();
                MainWindow = main;
                main.Show();
            }
            catch (Exception ex)
            {
                // Không mở được màn hình chính thì thoát hẳn, tránh tiến trình chạy ngầm không có cửa sổ
                MessageBox.Show("Không thể mở màn hình chính: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
