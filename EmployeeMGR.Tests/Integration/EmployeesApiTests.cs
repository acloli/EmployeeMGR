using System.Net;
using System.Net.Http.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using EmployeeMGR.Auth;

namespace EmployeeMGR.Tests.Integration;

public class EmployeesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmployeesApiTests(CustomWebApplicationFactory factory)
    {
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
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new { userName = "user", password = "test123" });
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.Token);

        var employeesResponse = await _client.GetAsync("/api/Employees?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, employeesResponse.StatusCode);
    }
    
    [Fact]
    public async Task DeleteEmployee_WithUserToken_Returns403()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new {userName = "user", password = "test123"});
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.Token);

        var deleteResponse = await _client.DeleteAsync("/api/Employees/1");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }
}