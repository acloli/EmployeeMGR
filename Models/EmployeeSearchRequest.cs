namespace EmployeeMGR.Models;

public class EmployeeSearchRequest
{
    public string? Name { get; set; }
    public string? Department { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}