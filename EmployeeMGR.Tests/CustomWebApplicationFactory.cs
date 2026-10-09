using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using EmployeeMGR.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EmployeeMGR.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtIssuer = "EmployeeMGR.Tests";
    private const string TestJwtAudience = "EmployeeMGR.Tests.Client";
    private const string TestJwtKey = "EmployeeMGR.Tests.SuperLongSecretKey.ForTestingOnly";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
        db.Database.EnsureCreated();
        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = TestJwtIssuer,
                ["Jwt:Audience"] = TestJwtAudience,
                ["Jwt:Key"] = TestJwtKey
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<EmployeeContext>();
            services.RemoveAll<DbContextOptions<EmployeeContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<EmployeeContext>>();
            services.AddSingleton(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();
                return connection;
            });
            services.AddDbContext<EmployeeContext>((provider, options) =>
                options.UseSqlite(provider.GetRequiredService<SqliteConnection>()));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = TestJwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = TestJwtAudience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey))
                };
            });
        });
    }
}
