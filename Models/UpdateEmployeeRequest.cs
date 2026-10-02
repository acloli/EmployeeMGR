using System.ComponentModel.DataAnnotations;

namespace EmployeeMGR.Models;

public class UpdateEmployeeRequest
{
    public int Id { get; set; }
    [Required] [StringLength(100)] public string? Name { get; set; }
    [Required] [StringLength(100)] public string? Email { get; set; }
    [Required] [StringLength(100)] public string? Phone { get; set; }
    [Required] [StringLength(100)] public string? Department { get; set; }
}

