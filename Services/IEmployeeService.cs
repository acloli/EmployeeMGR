using EmployeeMGR.Models;

namespace EmployeeMGR.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync();
    Task<PagedResponse<EmployeeResponse>> GetEmployeesAsync(string? name, string? department, int page, int pageSize);
    Task<EmployeeResponse?> GetEmployeeByIdAsync(int id);
    Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request);
    Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request);
    Task<bool> DeleteEmployeeAsync(int id);
}

