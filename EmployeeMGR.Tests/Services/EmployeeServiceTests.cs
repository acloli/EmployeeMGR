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
    public async Task CreateEmployees_WithSameTrimmedDepartment_ReusesDepartment()
    {
        var (connection, context) = await CreateContextAsync();
        await using var _ = connection;
        await using var __ = context;
        var service = new EmployeeService(context, NullLogger<EmployeeService>.Instance);

        var first = await service.CreateEmployeeAsync(new CreateEmployeeRequest
        {
            Name = "First", Department = " 開発部 "
        }, CancellationToken.None);
        var second = await service.CreateEmployeeAsync(new CreateEmployeeRequest
        {
            Name = "Second", Department = "開発部"
        }, CancellationToken.None);

        context.ChangeTracker.Clear();
        var department = await context.Departments.SingleAsync();
        var employees = await context.Employees.ToListAsync();
        Assert.All(employees, e =>
        {
            Assert.Equal(department.Id, e.DepartmentId);
            Assert.Equal("開発部", e.Department);
        });
        Assert.Equal("開発部", first.Department);
        Assert.Equal(department.Id, second.DepartmentInfo!.Id);
    }

    [Fact]
    public async Task UpdateEmployee_WhenDepartmentChanges_UpdatesRelationshipAndLegacyName()
    {
        var (connection, context) = await CreateContextAsync();
        await using var _ = connection;
        await using var __ = context;
        var service = new EmployeeService(context, NullLogger<EmployeeService>.Instance);
        var created = await service.CreateEmployeeAsync(new CreateEmployeeRequest
        {
            Name = "First", Department = "開発部"
        }, CancellationToken.None);

        var updated = await service.UpdateEmployeeAsync(created.Id, new UpdateEmployeeRequest
        {
            Id = created.Id, Name = "Updated", Department = " 営業部 "
        }, CancellationToken.None);

        Assert.True(updated);
        context.ChangeTracker.Clear();
        var employee = await context.Employees.Include(e => e.DepartmentInfo).SingleAsync();
        Assert.Equal("Updated", employee.Name);
        Assert.Equal("営業部", employee.Department);
        Assert.Equal("営業部", employee.DepartmentInfo!.Name);
        Assert.Equal(employee.DepartmentInfo.Id, employee.DepartmentId);
        Assert.NotEqual(created.DepartmentInfo!.Id, employee.DepartmentId);
    }

    [Fact]
    public async Task ReadEmployees_UsesMasterDepartmentAndFallsBackForLegacyEmployees()
    {
        var (connection, context) = await CreateContextAsync();
        await using var _ = connection;
        await using var __ = context;
        var linked = new Employee
        {
            Name = "Linked", Department = "Old name",
            DepartmentInfo = new Department { Name = "Master name" }
        };
        context.Employees.AddRange(linked, new Employee { Name = "Legacy", Department = "Legacy name" });
        await context.SaveChangesAsync();
        var id = linked.Id;
        context.ChangeTracker.Clear();
        var service = new EmployeeService(context, NullLogger<EmployeeService>.Instance);

        var detail = await service.GetEmployeeByIdAsync(id, CancellationToken.None);
        Assert.Equal("Master name", detail!.Department);
        Assert.Equal("Master name", detail.DepartmentInfo!.Name);

        var all = (await service.GetAllEmployeesAsync(CancellationToken.None)).ToList();
        Assert.Equal("Master name", all.Single(e => e.Name == "Linked").Department);
        var legacy = all.Single(e => e.Name == "Legacy");
        Assert.Equal("Legacy name", legacy.Department);
        Assert.Null(legacy.DepartmentInfo);

        var page = await service.GetEmployeesAsync(new EmployeeSearchRequest
        {
            Department = "Master", Page = 1, PageSize = 20
        }, CancellationToken.None);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal("Master name", Assert.Single(page.Items).Department);
        Assert.Empty(context.ChangeTracker.Entries());
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
        var result = await service.GetEmployeeByIdAsync(1, CancellationToken.None);

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
        var result = await service.GetEmployeeByIdAsync(999, CancellationToken.None);

        Assert.Null(result);
    }
}
