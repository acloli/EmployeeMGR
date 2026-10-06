using EmployeeMGR.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMGR.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeContext _context;

    public EmployeeService(EmployeeContext context)
    {
        _context = context;
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
        return await _context.Employees.Select(e => ToEmployeeResponse(e)).ToListAsync();
    }

    public async Task<PagedResponse<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request,
        CancellationToken token)
    {
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
        var employee = await _context.Employees.FindAsync(id);
        return employee == null ? null : ToEmployeeResponse(employee);
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        Employee employee = new Employee
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Department = request.Department
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return ToEmployeeResponse(employee);
    }

    public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request)
    {
        if (id != request.Id)
        {
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
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(id))
            {
                return false;
            }
            else
            {
                throw;
            }
        }

        return true;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return false;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}

