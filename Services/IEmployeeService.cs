using EmployeeMGR.Models;

namespace EmployeeMGR.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync();
    Task<EmployeeResponse?> GetEmployeeByIdAsync(int id);
    Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request);
    Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request);
    Task<bool> DeleteEmployeeAsync(int id);
}

