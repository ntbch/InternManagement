using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;
using InternManagement.Core.Services;

namespace InternManagement.Core.Tests;

public class OrganizationTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly DepartmentService _departments;
    private readonly MentorService _mentors;

    public OrganizationTests()
    {
        _departments = new DepartmentService(_db.Create);
        _mentors = new MentorService(_db.Create);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Department_DuplicateName_Throws()
    {
        Assert.Throws<ValidationException>(() => _departments.Add(new Department { Name = "kiểm thử" }));
    }

    [Fact]
    public void Department_InUse_CannotBeDeleted_EmptyCanBe()
    {
        Department qa = _departments.GetAll().Single(d => d.Name == "Kiểm thử");
        Assert.Throws<ValidationException>(() => _departments.Delete(qa.Id));

        var empty = new Department { Name = "Nhân sự" };
        _departments.Add(empty);
        _departments.Delete(empty.Id);
        Assert.DoesNotContain(_departments.GetAll(), d => d.Name == "Nhân sự");
    }

    [Fact]
    public void Mentor_Add_CreatesLoginAccountByEmail()
    {
        Department dev = _departments.GetAll().Single(d => d.Name == "Phát triển phần mềm");
        var mentor = new Mentor { FullName = "Mentor Mới", Email = "Moi@CongTy.vn", DepartmentId = dev.Id };
        _mentors.Add(mentor);

        User account = new AuthService(_db.Create).Login("moi@congty.vn", "123456");
        Assert.Equal(mentor.Id, account.MentorId);
    }

    [Fact]
    public void Mentor_WithInterns_CannotBeDeleted()
    {
        Mentor an = _mentors.GetAll("an@congty.vn").Single();
        Assert.Throws<ValidationException>(() => _mentors.Delete(an.Id));
    }
}
