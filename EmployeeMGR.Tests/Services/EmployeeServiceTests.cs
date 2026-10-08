using EmployeeMGR.Models;
using EmployeeMGR.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMGR.Tests.Services;

public class EmployeeServiceTests
{
    private static async Task<(SqliteConnection Connection, EmployeeContext Context)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<EmployeeContext>().UseSqlite(connection).Options;

        var context = new EmployeeContext(options);

        await context.Database.EnsureCreatedAsync();

        return (connection, context);
    }
}