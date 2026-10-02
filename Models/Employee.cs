namespace EmployeeMGR.Models;

public class Employee
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Department? Department { get; set; }
}
