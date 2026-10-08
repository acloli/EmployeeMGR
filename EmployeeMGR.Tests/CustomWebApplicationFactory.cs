using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EmployeeMGR.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "EmployeeMGR.Tests",
                ["Jwt:Audience"] = "EmployeeMGR.Tests.Client",
                ["Jwt:Key"] = "EmployeeMGR.Tests.SuperLongSecretKey.ForTestingOnly"
            });
        });
    }
}