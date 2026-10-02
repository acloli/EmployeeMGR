using Microsoft.EntityFrameworkCore;
using EmployeeMGR.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<EmployeeContext>(opt => opt.UseSqlite(
    builder.Configuration.GetConnectionString("EmployeeList") ??
    throw new InvalidOperationException("EmployeeList接続設定がありません。")
));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options => { options.DocumentPath = "/openapi/v1.json"; });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
