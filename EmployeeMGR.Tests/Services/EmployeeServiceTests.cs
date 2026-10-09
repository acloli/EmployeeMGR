using EmployeeMGR.Models;
using EmployeeMGR.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

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

    [Fact]
    public async Task GetEmployeeByIdAsync_WhenEmployeeExists_ReturnsEmployee()
    {
        var (connection, context) = await CreateContextAsync();

        await using var _ = connection;
        await using var __ = context;

        context.Employees.Add(new Employee
        {
            Name = "田中太郎",
            Email = "tanaka@example.com",
            Phone = "090-1234-5678",
            Department = "システム部",
        });

        await context.SaveChangesAsync();
        var service = new EmployeeService(context, NullLogger<EmployeeService>.Instance);
        var result = await service.GetEmployeeByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("田中太郎", result.Name);
        Assert.Equal("システム部", result.Department);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_WhenNotFound_ReturnsNull()
    {
        var (connection, context) = await CreateContextAsync();

        await using var _ = connection;
        await using var __ = context;

        var service = new EmployeeService(context, NullLogger<EmployeeService>.Instance);
        var result = await service.GetEmployeeByIdAsync(999);

        Assert.Null(result);
    }
}