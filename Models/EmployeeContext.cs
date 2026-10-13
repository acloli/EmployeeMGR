using Microsoft.EntityFrameworkCore;

namespace EmployeeMGR.Models;

public class EmployeeContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments => Set<Department>();

    public string DbPath { get; }

    public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "EmployeeMGR.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(d => d.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.DepartmentInfo)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

