using System;

namespace InternManagement.Core.Models
{
    /// <summary>Phiếu đánh giá của mentor cho thực tập sinh (thang điểm 0-10).</summary>
    public class Evaluation
    {
        public int Id { get; set; }
        public DateTime EvaluationDate { get; set; }
        public double AttitudeScore { get; set; }
        public double SkillScore { get; set; }
        public double TeamworkScore { get; set; }
        public string Comment { get; set; }

        public int InternId { get; set; }
        public Intern Intern { get; set; }

        public int MentorId { get; set; }
        public Mentor Mentor { get; set; }

        /// <summary>Điểm trung bình, làm tròn 2 chữ số. Không lưu vào CSDL.</summary>
        public double Average
        {
            get { return Math.Round((AttitudeScore + SkillScore + TeamworkScore) / 3, 2); }
        }

        /// <summary>Xếp loại theo điểm trung bình. Không lưu vào CSDL.</summary>
        public string Rank
        {
            get
            {
                double avg = Average;
                if (avg >= 8.5) return "Giỏi";
                if (avg >= 7.0) return "Khá";
                if (avg >= 5.0) return "Trung bình";
                return "Yếu";
            }
        }
    }
}
