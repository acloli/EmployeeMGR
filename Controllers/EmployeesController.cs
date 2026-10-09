using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeMGR.Models;
using EmployeeMGR.Services;

namespace EmployeeMGR.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: api/Employees?name=Tanaka
        // GET: api/Employees?department=Sales
        // GET: api/Employees?name=Sales&page=1&pageSize=20
        [HttpGet]
        public async Task<PagedResponse<EmployeeResponse>> GetEmployees([FromQuery] EmployeeSearchRequest request,
            CancellationToken token)
        {
            return await _employeeService.GetEmployeesAsync(request, token);
        }

        // GET: api/Employees/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<EmployeeResponse>>> GetAllEmployees(CancellationToken token)
        {
            var employees = await _employeeService.GetAllEmployeesAsync(token);
            return Ok(employees);
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployee(int id, CancellationToken token)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id, token);

            if (employee == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Employee not found",
                    detail: $"Employee with ID {id} was not found"
                );
            }

            return employee;
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, [FromBody] UpdateEmployeeRequest request,
            CancellationToken token)
        {
            if (id != request.Id)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid request",
                    detail: $"ID mismatch. Request ID: {request.Id}, URL ID: {id}"
                );
            }

            var updated = await _employeeService.UpdateEmployeeAsync(id, request, token);

            if (!updated)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Employee not found",
                    detail: $"Employee with ID {id} was not found"
                );
            }

            return NoContent();
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> PostEmployee([FromBody] CreateEmployeeRequest request,
            CancellationToken token)
        {
            var created = await _employeeService.CreateEmployeeAsync(request, token);
            return CreatedAtAction(nameof(GetEmployee), new { id = created.Id }, created);
        }

        // DELETE: api/Employees/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id, CancellationToken token)
        {
            var deleted = await _employeeService.DeleteEmployeeAsync(id, token);
            if (!deleted)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Employee not found",
                    detail: $"Employee with ID {id} was not found"
                );
            }

            return NoContent();
        }
    }
}
