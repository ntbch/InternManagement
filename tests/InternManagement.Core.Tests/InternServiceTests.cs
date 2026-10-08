using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;
using InternManagement.Core.Services;

namespace InternManagement.Core.Tests;

public class InternServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly InternService _interns;
    private readonly AuthService _auth;

    public InternServiceTests()
    {
        _interns = new InternService(_db.Create);
        _auth = new AuthService(_db.Create);
    }

    public void Dispose() => _db.Dispose();

    private Intern NewValidIntern()
    {
        Mentor an = new MentorService(_db.Create).GetAll("an@congty.vn").Single();
        return new Intern
        {
            Code = _interns.GenerateNextCode(),
            FullName = "Trịnh Văn Mới",
            Email = "moi@sv.edu.vn",
            Phone = "0911222333",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(3),
            Status = InternStatus.Pending,
            DepartmentId = an.DepartmentId,
            MentorId = an.Id
        };
    }

    [Fact]
    public void GenerateNextCode_FollowsHighestExistingCode()
    {
        Assert.Equal("TTS007", _interns.GenerateNextCode());
    }

    [Fact]
    public void Add_CreatesInternWithLoginAccount()
    {
        Intern intern = NewValidIntern();
        _interns.Add(intern);

        Assert.True(intern.Id > 0);
        User account = _auth.Login("TTS007", "123456");
        Assert.Equal(intern.Id, account.InternId);
    }

    [Fact]
    public void Add_DuplicateCodeOrEmail_Throws()
    {
        Intern dupCode = NewValidIntern();
        dupCode.Code = "tts001";
        Assert.Throws<ValidationException>(() => _interns.Add(dupCode));

        Intern dupEmail = NewValidIntern();
        dupEmail.Email = "HA.PT@sv.edu.vn";
        Assert.Throws<ValidationException>(() => _interns.Add(dupEmail));
    }

    [Fact]
    public void Add_InvalidData_Throws()
    {
        Intern badDates = NewValidIntern();
        badDates.EndDate = badDates.StartDate;
        Assert.Throws<ValidationException>(() => _interns.Add(badDates));

        Intern badEmail = NewValidIntern();
        badEmail.Email = "khong-phai-email";
        Assert.Throws<ValidationException>(() => _interns.Add(badEmail));

        Intern badPhone = NewValidIntern();
        badPhone.Phone = "12345";
        Assert.Throws<ValidationException>(() => _interns.Add(badPhone));

        Intern otherDeptMentor = NewValidIntern();
        otherDeptMentor.MentorId = new MentorService(_db.Create).GetAll("binh@congty.vn").Single().Id;
        Assert.Throws<ValidationException>(() => _interns.Add(otherDeptMentor));

        Assert.Equal(6, _interns.Search().Count);
    }

    [Fact]
    public void Search_FiltersByKeywordStatusAndMentor()
    {
        Assert.Equal(new[] { "TTS003" }, _interns.Search(keyword: "anh.vl").Select(i => i.Code));
        Assert.Equal(new[] { "TTS001", "TTS003", "TTS006" },
            _interns.Search(keyword: "PHENIKAA").Select(i => i.Code));
        Assert.Equal(new[] { "TTS001", "TTS002", "TTS005" },
            _interns.Search(status: InternStatus.Active).Select(i => i.Code));

        int anId = new MentorService(_db.Create).GetAll("an@congty.vn").Single().Id;
        Assert.Equal(new[] { "TTS001", "TTS002" }, _interns.Search(mentorId: anId).Select(i => i.Code));
    }

    [Fact]
    public void Update_ChangingCodeRenamesLoginAccount()
    {
        Intern intern = _interns.Search(keyword: "TTS002").Single();
        intern.Code = "TTS099";
        _interns.Update(intern);

        Assert.Throws<ValidationException>(() => _auth.Login("TTS002", "123456"));
        Assert.Equal(intern.Id, _auth.Login("TTS099", "123456").InternId);
    }

    [Fact]
    public void Delete_RemovesTasksEvaluationsAndAccount()
    {
        Intern intern = _interns.Search(keyword: "TTS001").Single();
        _interns.Delete(intern.Id);

        using var db = _db.Create();
        Assert.False(db.Tasks.Any(t => t.InternId == intern.Id));
        Assert.False(db.Evaluations.Any(e => e.InternId == intern.Id));
        Assert.False(db.Users.Any(u => u.InternId == intern.Id));
    }
}
