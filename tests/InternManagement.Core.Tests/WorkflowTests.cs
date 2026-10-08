using InternManagement.Core.Exceptions;
using InternManagement.Core.Helpers;
using InternManagement.Core.Models;
using InternManagement.Core.Services;

namespace InternManagement.Core.Tests;

public class WorkflowTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly InternService _interns;
    private readonly TaskService _tasks;
    private readonly EvaluationService _evaluations;

    public WorkflowTests()
    {
        _interns = new InternService(_db.Create);
        _tasks = new TaskService(_db.Create);
        _evaluations = new EvaluationService(_db.Create);
    }

    public void Dispose() => _db.Dispose();

    private Intern Intern(string code) => _interns.Search(keyword: code).Single();

    [Fact]
    public void UpdateStatus_OnlyOwnerInternCanChangeTask()
    {
        Intern ha = Intern("TTS001");
        Intern minh = Intern("TTS002");
        TaskItem task = _tasks.GetTasks(internId: ha.Id).First(t => t.Status != TaskItemStatus.Done);

        Assert.Throws<ValidationException>(() => _tasks.UpdateStatus(task.Id, TaskItemStatus.Done, minh.Id));

        _tasks.UpdateStatus(task.Id, TaskItemStatus.Done, ha.Id);
        Assert.Equal(TaskItemStatus.Done, _tasks.GetTasks(internId: ha.Id).Single(t => t.Id == task.Id).Status);
    }

    [Fact]
    public void Task_DueBeforeAssigned_Throws()
    {
        var task = new TaskItem
        {
            InternId = Intern("TTS001").Id,
            Title = "Việc mới",
            AssignedDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(-1)
        };
        Assert.Throws<ValidationException>(() => _tasks.Add(task));
    }

    [Fact]
    public void IsOverdue_OnlyForUnfinishedPastDueTasks()
    {
        Assert.True(new TaskItem { DueDate = DateTime.Today.AddDays(-1), Status = TaskItemStatus.InProgress }.IsOverdue);
        Assert.False(new TaskItem { DueDate = DateTime.Today.AddDays(-1), Status = TaskItemStatus.Done }.IsOverdue);
        Assert.False(new TaskItem { DueDate = DateTime.Today, Status = TaskItemStatus.Todo }.IsOverdue);
    }

    [Fact]
    public void Evaluation_OnlyAssignedMentorWithValidScores()
    {
        Intern ha = Intern("TTS001"); // mentor: An
        int binhId = new MentorService(_db.Create).GetAll("binh@congty.vn").Single().Id;

        var byOtherMentor = new Evaluation
        {
            InternId = ha.Id, MentorId = binhId, EvaluationDate = DateTime.Today,
            AttitudeScore = 8, SkillScore = 8, TeamworkScore = 8
        };
        Assert.Throws<ValidationException>(() => _evaluations.Add(byOtherMentor));

        var outOfRange = new Evaluation
        {
            InternId = ha.Id, MentorId = ha.MentorId!.Value, EvaluationDate = DateTime.Today,
            AttitudeScore = 11, SkillScore = 8, TeamworkScore = 8
        };
        Assert.Throws<ValidationException>(() => _evaluations.Add(outOfRange));

        var ok = new Evaluation
        {
            InternId = ha.Id, MentorId = ha.MentorId!.Value, EvaluationDate = DateTime.Today,
            AttitudeScore = 9, SkillScore = 8, TeamworkScore = 8.5
        };
        _evaluations.Add(ok);
        Assert.Equal(2, _evaluations.GetEvaluations(internId: ha.Id).Count);
    }

    [Theory]
    [InlineData(9, 8.5, 8, 8.5, "Giỏi")]
    [InlineData(7, 7, 7, 7, "Khá")]
    [InlineData(5, 5, 5, 5, "Trung bình")]
    [InlineData(4, 5, 5.5, 4.83, "Yếu")]
    public void Evaluation_AverageAndRank(double a, double s, double t, double avg, string rank)
    {
        var e = new Evaluation { AttitudeScore = a, SkillScore = s, TeamworkScore = t };
        Assert.Equal(avg, e.Average);
        Assert.Equal(rank, e.Rank);
    }

    [Fact]
    public void Dashboard_SummarizesSeedData()
    {
        DashboardSummary s = new DashboardService(_db.Create).GetSummary();

        Assert.Equal(6, s.TotalInterns);
        Assert.Equal(3, s.ActiveInterns);
        Assert.Equal(3, s.TotalMentors);
        Assert.Equal(2, s.OverdueTasks);
        Assert.Equal(6, s.InternsByStatus.Sum(c => c.Value));
        Assert.Equal(3, s.InternsByDepartment.Single(c => c.Label == "Phát triển phần mềm").Value);
        Assert.NotNull(s.AverageScore);
    }

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
    [InlineData(null, "")]
    public void Csv_EscapesSpecialCharacters(string input, string expected)
    {
        Assert.Equal(expected, CsvExporter.Escape(input));
    }
}
