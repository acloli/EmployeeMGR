using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using EmployeeMGR.Auth;
using EmployeeMGR.Models;
using EmployeeMGR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EmployeeMGR.Tests.Integration;

public class EmployeesApiTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmployeesApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEmployees_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Employees?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_WithToken_ReturnsOk()
    {
        await AuthenticateAsync(_client, "user");

        var response = await _client.GetAsync("/api/Employees?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_WithUserToken_Returns403()
    {
        await AuthenticateAsync(_client, "user");

        var response = await _client.DeleteAsync("/api/Employees/1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Employee_WhenNotFound_Returns404ProblemDetails(string method)
    {
        await AuthenticateAsync(_client, "admin");
        const int missingId = int.MaxValue;
        using var request = new HttpRequestMessage(new HttpMethod(method), $"/api/Employees/{missingId}");
        if (method == "PUT")
        {
            request.Content = JsonContent.Create(new UpdateEmployeeRequest
            {
                Id = missingId,
                Name = "Test Employee",
                Email = "test@example.com",
                Phone = "090-1234-5678",
                Department = "Testing"
            });
        }

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Employee not found", problem.Title);
        Assert.Contains(missingId.ToString(), problem.Detail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Name exceeds maximum length")]
    public async Task CreateEmployee_WithInvalidRequest_Returns400ValidationProblemDetails(string scenario)
    {
        await AuthenticateAsync(_client, "admin");
        var request = new CreateEmployeeRequest
        {
            Name = scenario == "" ? "" : new string('a', 101),
            Email = "test@example.com",
            Phone = "090-1234-5678",
            Department = "Testing"
        };

        using var response = await _client.PostAsJsonAsync("/api/Employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
        Assert.True(problem.Errors.TryGetValue("Name", out var errors));
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task UpdateEmployee_WithMismatchedId_Returns400ProblemDetails()
    {
        await AuthenticateAsync(_client, "admin");
        using var response = await _client.PutAsJsonAsync("/api/Employees/1", new UpdateEmployeeRequest
        {
            Id = 2,
            Name = "Test Employee",
            Email = "test@example.com",
            Phone = "090-1234-5678",
            Department = "Testing"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Invalid request", problem.Title);
    }

    [Fact]
    public async Task GetEmployees_WhenUnhandledExceptionOccurs_ReturnsSafe500ProblemDetails()
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEmployeeService>();
                services.AddScoped<IEmployeeService, ThrowingEmployeeService>();
            }));
        using var client = factory.CreateClient();
        await AuthenticateAsync(client, "user");

        using var response = await client.GetAsync("/api/Employees?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        Assert.Equal(500, root.GetProperty("status").GetInt32());
        Assert.Equal("Internal Server Error", root.GetProperty("title").GetString());
        Assert.Equal("An unexpected error occurred.", root.GetProperty("detail").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("traceId").GetString()));
        Assert.DoesNotContain(ThrowingEmployeeService.ExceptionMessage, body);
        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("stackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQLite", body);
        Assert.DoesNotContain("/private/", body);
    }

    [Fact]
    public async Task CreateEmployee_WithAdminToken_Returns201AndLocationOfPersistedEmployee()
    {
        await AuthenticateAsync(_client, "admin");
        var request = new CreateEmployeeRequest
        {
            Name = "Created Employee",
            Email = "created@example.com",
            Phone = "090-1234-5678",
            Department = "Testing"
        };

        using var response = await _client.PostAsJsonAsync("/api/Employees", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var employee = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotNull(employee);
        Assert.True(employee.Id > 0);
        Assert.Equal(request.Name, employee.Name);
        Assert.Equal(request.Email, employee.Email);
        Assert.Equal(request.Phone, employee.Phone);
        Assert.Equal(request.Department, employee.Department);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        Assert.Equal($"/api/Employees/{employee.Id}",
            location.IsAbsoluteUri ? location.AbsolutePath : location.OriginalString);

        using var getResponse = await _client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var persisted = await getResponse.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotNull(persisted);
        Assert.Equal(employee.Id, persisted.Id);
        Assert.Equal(request.Name, persisted.Name);
        Assert.Equal(request.Email, persisted.Email);
        Assert.Equal(request.Phone, persisted.Phone);
        Assert.Equal(request.Department, persisted.Department);
    }

    private static async Task AuthenticateAsync(HttpClient client, string userName)
    {
        using var response = await client.PostAsJsonAsync("/api/Auth/login", new { userName, password = "test123" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
    }

    public void Dispose() => _client.Dispose();

    private sealed class ThrowingEmployeeService : IEmployeeService
    {
        public const string ExceptionMessage = "SQLite failure at /private/test/EmployeeList.db: confidential diagnostic";

        public Task<PagedResponse<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request,
            CancellationToken token) => throw new InvalidOperationException(ExceptionMessage);

        public Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync(CancellationToken token) =>
            throw new NotSupportedException();

        public Task<EmployeeResponse?> GetEmployeeByIdAsync(int id, CancellationToken token) =>
            throw new NotSupportedException();

        public Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken token) =>
            throw new NotSupportedException();

        public Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, CancellationToken token) =>
            throw new NotSupportedException();

        public Task<bool> DeleteEmployeeAsync(int id, CancellationToken token) => throw new NotSupportedException();
    }
}
