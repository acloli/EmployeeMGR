using EmployeeMGR.Models;

namespace EmployeeMGR.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync(CancellationToken token);
    Task<PagedResponse<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request, CancellationToken token);
    Task<EmployeeResponse?> GetEmployeeByIdAsync(int id, CancellationToken token);
    Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken token);
    Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, CancellationToken token);
    Task<bool> DeleteEmployeeAsync(int id, CancellationToken token);
}

