using System;
using System.Collections.Generic;
using System.Linq;
using InternManagement.Core.Data;
using InternManagement.Core.Exceptions;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Core.Services
{
    /// <summary>Phiếu đánh giá thực tập sinh. Chỉ mentor đang hướng dẫn mới được đánh giá.</summary>
    public class EvaluationService
    {
        private readonly Func<AppDbContext> _createDb;

        public EvaluationService(Func<AppDbContext> createDb)
        {
            _createDb = createDb;
        }

        public List<Evaluation> GetEvaluations(int? internId = null, int? mentorId = null)
        {
            using (AppDbContext db = _createDb())
            {
                IQueryable<Evaluation> query = db.Evaluations
                    .Include(e => e.Intern)
                    .Include(e => e.Mentor);

                if (internId.HasValue)
                    query = query.Where(e => e.InternId == internId.Value);
                if (mentorId.HasValue)
                    query = query.Where(e => e.MentorId == mentorId.Value);

                return query.OrderByDescending(e => e.EvaluationDate).ToList();
            }
        }

        public void Add(Evaluation evaluation)
        {
            using (AppDbContext db = _createDb())
            {
                var entity = new Evaluation();
                Apply(db, entity, evaluation);
                db.Evaluations.Add(entity);
                db.SaveChanges();
                evaluation.Id = entity.Id;
            }
        }

        public void Update(Evaluation evaluation)
        {
            using (AppDbContext db = _createDb())
            {
                Evaluation entity = db.Evaluations.Find(evaluation.Id);
                if (entity == null)
                    throw new ValidationException("Phiếu đánh giá không tồn tại.");
                Apply(db, entity, evaluation);
                db.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (AppDbContext db = _createDb())
            {
                Evaluation entity = db.Evaluations.Find(id);
                if (entity == null)
                    return;
                db.Evaluations.Remove(entity);
                db.SaveChanges();
            }
        }

        private static void Apply(AppDbContext db, Evaluation entity, Evaluation input)
        {
            Intern intern = db.Interns.Find(input.InternId);
            if (intern == null)
                throw new ValidationException("Vui lòng chọn thực tập sinh.");
            if (intern.MentorId != input.MentorId)
                throw new ValidationException("Chỉ mentor đang hướng dẫn thực tập sinh này mới được đánh giá.");
            Validator.Score(input.AttitudeScore, "Điểm thái độ");
            Validator.Score(input.SkillScore, "Điểm kỹ năng");
            Validator.Score(input.TeamworkScore, "Điểm làm việc nhóm");
            if (input.EvaluationDate.Date > DateTime.Today)
                throw new ValidationException("Ngày đánh giá không được ở tương lai.");

            entity.InternId = input.InternId;
            entity.MentorId = input.MentorId;
            entity.EvaluationDate = input.EvaluationDate.Date;
            entity.AttitudeScore = input.AttitudeScore;
            entity.SkillScore = input.SkillScore;
            entity.TeamworkScore = input.TeamworkScore;
            entity.Comment = Validator.Optional(input.Comment);
        }
    }
}
