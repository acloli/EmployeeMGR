using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using EmployeeMGR.Models;
using EmployeeMGR.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("Jwt:Key is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
                  ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options => { options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0; });

builder.Services.AddDbContext<EmployeeContext>(opt => opt.UseSqlite(
    builder.Configuration.GetConnectionString("EmployeeList") ??
    throw new InvalidOperationException("EmployeeList接続設定がありません。")
));

builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options => { options.DocumentPath = "/openapi/v1.json"; });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
