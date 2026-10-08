using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using InternManagement.Core.Services;

namespace InternManagement.Wpf.Views
{
    /// <summary>
    /// Màn hình Tổng quan (Dashboard):
    /// - 6 thẻ thống kê số liệu tổng quan (WrapPanel).
    /// - 2 biểu đồ cột vẽ thủ công trên Canvas bằng các đối tượng Shape (Rectangle).
    /// </summary>
    public partial class DashboardView : UserControl
    {
        private DashboardSummary _summary;

        public DashboardView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _summary = AppServices.Dashboard.GetSummary();
            DisplaySummary();
        }

        private void DisplaySummary()
        {
            if (_summary == null)
                return;

            txtTotalInterns.Text = _summary.TotalInterns.ToString();
            txtActiveInterns.Text = _summary.ActiveInterns.ToString();
            txtTotalMentors.Text = _summary.TotalMentors.ToString();
            txtTotalDepartments.Text = _summary.TotalDepartments.ToString();

            txtOverdueTasks.Text = _summary.OverdueTasks.ToString();
            if (_summary.OverdueTasks > 0)
            {
                txtOverdueTasks.Foreground = (Brush)TryFindResource("DangerBrush");
            }

            if (_summary.AverageScore.HasValue)
            {
                txtAverageScore.Text = _summary.AverageScore.Value.ToString("0.00");
            }
            else
            {
                txtAverageScore.Text = "—";
            }

            RedrawCharts();
        }

        private void RedrawCharts()
        {
            if (_summary == null)
                return;

            Brush primary = (Brush)TryFindResource("PrimaryBrush") ?? Brushes.DodgerBlue;
            Brush success = (Brush)TryFindResource("SuccessBrush") ?? Brushes.ForestGreen;

            DrawBarChart(canvasStatus, _summary.InternsByStatus, primary);
            DrawBarChart(canvasDepartment, _summary.InternsByDepartment, success);
        }

        private void CanvasStatus_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_summary != null)
            {
                Brush primary = (Brush)TryFindResource("PrimaryBrush") ?? Brushes.DodgerBlue;
                DrawBarChart(canvasStatus, _summary.InternsByStatus, primary);
            }
        }

        private void CanvasDepartment_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_summary != null)
            {
                Brush success = (Brush)TryFindResource("SuccessBrush") ?? Brushes.ForestGreen;
                DrawBarChart(canvasDepartment, _summary.InternsByDepartment, success);
            }
        }

        /// <summary>
        /// Đồ họa 2D trong WPF — Shape (Rectangle) đặt trên Canvas theo tọa độ.
        /// Hàm vẽ biểu đồ cột: xóa hình cũ, tính giá trị lớn nhất, chia đều chiều rộng cột,
        /// vẽ cột Rectangle với chiều cao tỷ lệ, TextBlock giá trị ở trên và TextBlock nhãn ở dưới.
        /// </summary>
        private void DrawBarChart(Canvas canvas, List<ChartItem> items, Brush brush)
        {
            canvas.Children.Clear();

            if (items == null || items.Count == 0)
                return;

            // ActualWidth bằng 0 trước khi giao diện hoàn tất bố cục (layout pass)
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;
            if (canvasWidth <= 0 || canvasHeight <= 0)
                return;

            // Tính giá trị lớn nhất (bảo vệ trường hợp tất cả bằng 0 thì coi max = 1)
            int max = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Value > max)
                    max = items[i].Value;
            }
            if (max == 0)
                max = 1;

            int count = items.Count;
            double labelHeight = 38.0;
            double valueHeight = 22.0;
            double availableChartHeight = canvasHeight - labelHeight - valueHeight;
            if (availableChartHeight <= 10.0)
                return;

            double baselineY = canvasHeight - labelHeight;
            double slotWidth = canvasWidth / count;
            // Độ rộng cột tỷ lệ theo mỗi ô, tối đa 50px và tối thiểu 12px
            double barWidth = Math.Max(12.0, Math.Min(50.0, slotWidth * 0.55));
            double barOffset = (slotWidth - barWidth) / 2.0;

            // Đường chuẩn chân biểu đồ
            Line baseline = new Line
            {
                X1 = 0,
                Y1 = baselineY,
                X2 = canvasWidth,
                Y2 = baselineY,
                Stroke = (Brush)TryFindResource("BorderBrush") ?? Brushes.LightGray,
                StrokeThickness = 1
            };
            canvas.Children.Add(baseline);

            for (int i = 0; i < count; i++)
            {
                ChartItem item = items[i];
                double slotLeft = i * slotWidth;
                double barLeft = slotLeft + barOffset;

                // Chiều cao cột tỷ lệ theo giá trị
                double barHeight = (item.Value / (double)max) * availableChartHeight;
                if (item.Value > 0 && barHeight < 4.0)
                    barHeight = 4.0; // chiều cao tối thiểu để nhìn thấy cột khi giá trị > 0

                double barTop = baselineY - barHeight;

                // Hình chữ nhật (Shape Rectangle) làm cột biểu đồ
                Rectangle rect = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(0, barHeight),
                    Fill = brush,
                    RadiusX = 3,
                    RadiusY = 3
                };
                Canvas.SetLeft(rect, barLeft);
                Canvas.SetTop(rect, barTop);
                canvas.Children.Add(rect);

                // Số liệu hiển thị bên trên cột
                TextBlock txtValue = new TextBlock
                {
                    Text = item.Value.ToString(),
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    Foreground = brush,
                    TextAlignment = TextAlignment.Center,
                    Width = slotWidth
                };
                Canvas.SetLeft(txtValue, slotLeft);
                Canvas.SetTop(txtValue, Math.Max(0, barTop - valueHeight));
                canvas.Children.Add(txtValue);

                // Nhãn danh mục hiển thị bên dưới cột (cho phép xuống dòng nếu dài)
                TextBlock txtLabel = new TextBlock
                {
                    Text = item.Label,
                    FontSize = 11,
                    Foreground = (Brush)TryFindResource("MutedBrush") ?? Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Width = slotWidth
                };
                Canvas.SetLeft(txtLabel, slotLeft);
                Canvas.SetTop(txtLabel, baselineY + 4);
                canvas.Children.Add(txtLabel);
            }
        }
    }
}
