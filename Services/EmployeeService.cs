using EmployeeMGR.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EmployeeMGR.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeContext _context;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(EmployeeContext context, ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static readonly Expression<Func<Employee, EmployeeResponse>> EmployeeResponseProjection = employee =>
        new EmployeeResponse
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Phone = employee.Phone,
            Department = employee.DepartmentInfo != null ? employee.DepartmentInfo.Name : employee.Department,
            DepartmentInfo = employee.DepartmentInfo != null
                ? new DepartmentResponse { Id = employee.DepartmentInfo.Id, Name = employee.DepartmentInfo.Name }
                : null
        };

    private static readonly Func<Employee, EmployeeResponse> ToEmployeeResponse = EmployeeResponseProjection.Compile();

    public async Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync(CancellationToken token)
    {
        _logger.LogDebug("Getting all employees");
        var employees = await _context.Employees.AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(EmployeeResponseProjection)
            .ToListAsync(token);
        _logger.LogDebug("Retrieved {EmployeeCount} employees", employees.Count);
        return employees;
    }

    public async Task<PagedResponse<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request,
        CancellationToken token)
    {
        _logger.LogDebug(
            "Getting employees. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);
        var query = _context.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(e => e.Name!.Contains(request.Name));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(e => (e.DepartmentInfo != null
                ? e.DepartmentInfo.Name
                : e.Department)!.Contains(request.Department));
        }

        var totalCount = await query.CountAsync(token);
        var pageCount = (int)Math.Ceiling((double)totalCount / request.PageSize);

        if (request.Page < 1 || request.Page > pageCount)
        {
            request.Page = 1;
        }

        var employees = await query.OrderBy(employee => employee.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(EmployeeResponseProjection)
            .ToListAsync(token);

        _logger.LogDebug(
            "Retrieved {EmployeeCount} employees. Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}",
            employees.Count, request.Page, request.PageSize, totalCount);
        return new PagedResponse<EmployeeResponse>
        {
            Items = employees,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<EmployeeResponse?> GetEmployeeByIdAsync(int id, CancellationToken token)
    {
        _logger.LogDebug("Getting employee {EmployeeId}", id);
        var employee = await _context.Employees.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(EmployeeResponseProjection)
            .SingleOrDefaultAsync(token);
        if (employee == null)
        {
            _logger.LogWarning("Employee {EmployeeId} was not found", id);
            return null;
        }

        _logger.LogDebug("Retrieved employee {EmployeeId}", id);
        return employee;
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken token)
    {
        _logger.LogDebug("Creating employee");

        Department department = await GetOrCreateDepartmentAsync(request.Department!, token);

        Employee employee = new Employee
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Department = department.Name,
            DepartmentInfo = department
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(token);
        _logger.LogInformation("Employee {EmployeeId} created", employee.Id);
        return ToEmployeeResponse(employee);
    }

    public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, CancellationToken token)
    {
        _logger.LogDebug("Updating employee {EmployeeId}", id);
        if (id != request.Id)
        {
            _logger.LogWarning(
                "Employee update rejected. RouteId: {RouteId}, RequestId: {RequestId}",
                id, request.Id);
            return false;
        }

        var employee = await _context.Employees.FindAsync([id], token);
        if (employee == null)
        {
            _logger.LogWarning("Employee {EmployeeId} was not found for update", id);
            return false;
        }

        var department = await GetOrCreateDepartmentAsync(request.Department!, token);
        employee.Name = request.Name;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.Department = department.Name;
        employee.DepartmentInfo = department;
        try
        {
            await _context.SaveChangesAsync(token);
            _logger.LogInformation("Employee {EmployeeId} updated", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employees.AnyAsync(e => e.Id == id, token))
            {
                _logger.LogWarning("Employee {EmployeeId} was not found for update", id);
                return false;
            }

            throw;
        }

        return true;
    }

    public async Task<bool> DeleteEmployeeAsync(int id, CancellationToken token)
    {
        _logger.LogDebug("Deleting employee {EmployeeId}", id);
        var employee = await _context.Employees.FindAsync([id], token);
        if (employee == null)
        {
            _logger.LogWarning("Employee {EmployeeId} was not found for deletion", id);
            return false;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(token);
        _logger.LogInformation("Employee {EmployeeId} deleted", id);
        return true;
    }

    private async Task<Department> GetOrCreateDepartmentAsync(string departmentName, CancellationToken token)
    {
        var name = departmentName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("部署名が空です。");
        }

        var department = await _context.Departments.SingleOrDefaultAsync(dep => dep.Name == name, token);
        if (department != null)
        {
            return department;
        }

        department = new Department { Name = name };
        _context.Departments.Add(department);
        return department;
    }
}
