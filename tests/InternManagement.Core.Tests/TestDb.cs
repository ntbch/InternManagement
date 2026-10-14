using InternManagement.Core.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Core.Tests;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = Create();
        DbSeeder.Initialize(db);
    }

    public AppDbContext Create() => new AppDbContext(_options);

    public void Dispose() => _connection.Dispose();
}
