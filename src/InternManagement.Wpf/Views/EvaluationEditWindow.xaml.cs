using System;
using System.Collections.Generic;
using System.Windows;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;

namespace InternManagement.Wpf.Views
{
    public partial class EvaluationEditWindow : Window
    {
        private readonly Evaluation _evaluation;
        private readonly bool _isNew;

        public EvaluationEditWindow(Evaluation evaluation, List<Intern> interns)
        {
            InitializeComponent();

            _isNew = evaluation == null;
            _evaluation = evaluation ?? new Evaluation();

            if (_isNew)
            {
                _evaluation.EvaluationDate = DateTime.Today;
                _evaluation.AttitudeScore = 8.0;
                _evaluation.SkillScore = 8.0;
                _evaluation.TeamworkScore = 8.0;
            }

            Title = _isNew ? "Thêm đánh giá" : "Sửa đánh giá";

            cboIntern.ItemsSource = interns;
            if (_isNew && interns != null && interns.Count > 0)
            {
                _evaluation.InternId = interns[0].Id;
            }

            DataContext = _evaluation;

            UpdateAverageAndRank();
            cboIntern.Focus();
        }

        private void ScoreSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateAverageAndRank();
        }

        private void UpdateAverageAndRank()
        {
            if (txtAverageRank == null || sldAttitude == null || sldSkill == null || sldTeamwork == null)
                return;

            var preview = new Evaluation
            {
                AttitudeScore = sldAttitude.Value,
                SkillScore = sldSkill.Value,
                TeamworkScore = sldTeamwork.Value
            };
            txtAverageRank.Text = string.Format("Điểm trung bình: {0:0.00} – Xếp loại: {1}",
                preview.Average, preview.Rank);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Gán MentorId từ Session của mentor đang đăng nhập, vì người đánh giá phải là
            // mentor hiện tại và EvaluationService kiểm tra intern.MentorId == evaluation.MentorId.
            if (!Session.MentorId.HasValue)
            {
                MessageBox.Show("Không tìm thấy thông tin mentor đăng nhập.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _evaluation.MentorId = Session.MentorId.Value;

            try
            {
                if (_isNew)
                    AppServices.Evaluations.Add(_evaluation);
                else
                    AppServices.Evaluations.Update(_evaluation);

                DialogResult = true;
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
