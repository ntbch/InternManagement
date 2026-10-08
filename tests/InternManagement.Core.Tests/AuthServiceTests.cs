using InternManagement.Core.Exceptions;
using InternManagement.Core.Models;
using InternManagement.Core.Services;

namespace InternManagement.Core.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly AuthService _auth;

    public AuthServiceTests() => _auth = new AuthService(_db.Create);

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Login_SeededAccounts_ReturnsRoleAndLinkedProfile()
    {
        Assert.Equal(UserRole.Admin, _auth.Login("admin", "admin123").Role);

        User mentor = _auth.Login("an@congty.vn", "123456");
        Assert.Equal(UserRole.Mentor, mentor.Role);
        Assert.Equal("Nguyễn Văn An", mentor.DisplayName);

        User intern = _auth.Login("tts001", "123456"); // không phân biệt hoa thường
        Assert.Equal(UserRole.Intern, intern.Role);
        Assert.Equal("TTS001", intern.Intern.Code);
    }

    [Theory]
    [InlineData("admin", "sai-mat-khau")]
    [InlineData("khong-ton-tai", "123456")]
    [InlineData("", "")]
    public void Login_InvalidCredentials_Throws(string username, string password)
    {
        Assert.Throws<ValidationException>(() => _auth.Login(username, password));
    }

    [Fact]
    public void ChangePassword_ReplacesOldPassword()
    {
        User admin = _auth.Login("admin", "admin123");
        _auth.ChangePassword(admin.Id, "admin123", "matkhaumoi", "matkhaumoi");

        Assert.Throws<ValidationException>(() => _auth.Login("admin", "admin123"));
        Assert.Equal(admin.Id, _auth.Login("admin", "matkhaumoi").Id);
    }

    [Theory]
    [InlineData("sai", "abcdef", "abcdef")]     // sai mật khẩu cũ
    [InlineData("admin123", "abc", "abc")]      // quá ngắn
    [InlineData("admin123", "abcdef", "abcxyz")] // xác nhận không khớp
    public void ChangePassword_InvalidInput_Throws(string oldPw, string newPw, string confirm)
    {
        User admin = _auth.Login("admin", "admin123");
        Assert.Throws<ValidationException>(() => _auth.ChangePassword(admin.Id, oldPw, newPw, confirm));
        Assert.Equal(admin.Id, _auth.Login("admin", "admin123").Id);
    }
}
