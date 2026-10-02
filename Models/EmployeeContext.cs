using Microsoft.EntityFrameworkCore;

namespace EmployeeMGR.Models;

public class EmployeeContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }

    public string DbPath { get; }

    public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "EmployeeMGR.db");
    }
}

