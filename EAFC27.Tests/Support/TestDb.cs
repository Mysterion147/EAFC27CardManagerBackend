using EAFC27.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EAFC27.Tests.Support;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        Context = new AppDbContext(Options());
        Context.Database.EnsureCreated();
    }

    // second context to assert on what was persisted
    public AppDbContext NewContext() => new(Options());

    private DbContextOptions<AppDbContext> Options() =>
        new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
