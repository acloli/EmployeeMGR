using EmployeeMGR.Models;
using Microsoft.EntityFrameworkCore;

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

    private static EmployeeResponse ToEmployeeResponse(Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Phone = employee.Phone,
            Department = employee.Department
        };
    }

    public async Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync()
    {
        _logger.LogDebug("Getting all employees");
        var employees = await _context.Employees.Select(e => ToEmployeeResponse(e)).ToListAsync();
        _logger.LogDebug("Retrieved {EmployeeCount} employees", employees.Count);
        return employees;
    }

    public async Task<PagedResponse<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request,
        CancellationToken token)
    {
        _logger.LogDebug(
            "Getting employees. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);
        var query = _context.Employees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(e => e.Name!.Contains(request.Name));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(e => e.Department!.Contains(request.Department));
        }

        var employees = await query.Select(e => ToEmployeeResponse(e)).ToListAsync(token);
        var totalCount = await query.CountAsync(token);
        var pageCount = (int)Math.Ceiling((double)totalCount / request.PageSize);

        if (request.Page < 1 || request.Page > pageCount)
        {
            request.Page = 1;
        }

        var pagedEmployees = employees
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        _logger.LogDebug(
            "Retrieved {EmployeeCount} employees. Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}",
            pagedEmployees.Count, request.Page, request.PageSize, totalCount);
        return new PagedResponse<EmployeeResponse>
        {
            Items = pagedEmployees,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<EmployeeResponse?> GetEmployeeByIdAsync(int id)
    {
        _logger.LogDebug("Getting employee {EmployeeId}", id);
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            _logger.LogWarning("Employee {EmployeeId} was not found", id);
            return null;
        }

        _logger.LogDebug("Retrieved employee {EmployeeId}", id);
        return ToEmployeeResponse(employee);
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        _logger.LogDebug("Creating employee");
        Employee employee = new Employee
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Department = request.Department
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Employee {EmployeeId} created", employee.Id);
        return ToEmployeeResponse(employee);
    }

    public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request)
    {
        _logger.LogDebug("Updating employee {EmployeeId}", id);
        if (id != request.Id)
        {
            _logger.LogWarning(
                "Employee update rejected. RouteId: {RouteId}, RequestId: {RequestId}",
                id, request.Id);
            return false;
        }

        Employee employee = new()
        {
            Id = request.Id,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Department = request.Department
        };
        _context.Entry(employee).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee {EmployeeId} updated", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(id))
            {
                _logger.LogWarning("Employee {EmployeeId} was not found for update", id);
                return false;
            }

            throw;
        }

        return true;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        _logger.LogDebug("Deleting employee {EmployeeId}", id);
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            _logger.LogWarning("Employee {EmployeeId} was not found for deletion", id);
            return false;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Employee {EmployeeId} deleted", id);
        return true;
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}

