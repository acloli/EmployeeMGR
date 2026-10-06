using Microsoft.AspNetCore.Mvc;
using EmployeeMGR.Models;
using EmployeeMGR.Services;

namespace EmployeeMGR.Controllers
{
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
        public async Task<PagedResponse<EmployeeResponse>> GetEmployees([FromQuery] EmployeeSearchRequest request)
        {
            return await _employeeService.GetEmployeesAsync(request);
        }

        // GET: api/Employees/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<EmployeeResponse>>> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            Employee employee = new()
            {
                Id = request.Id,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Department = request.Department
            };

            var updated = await _employeeService.UpdateEmployeeAsync(id, request);

            if (!updated)
            {
                return NotFound();
            }
            else
            {
                return NoContent();
            }
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> PostEmployee([FromBody] CreateEmployeeRequest request)
        {
            var created = await _employeeService.CreateEmployeeAsync(request);
            return created;
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var deleted = await _employeeService.DeleteEmployeeAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            else
            {
                return NoContent();
            }
        }
    }
}
