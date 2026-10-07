using EmployeeMGR.Auth;

namespace EmployeeMGR.Services;

public interface ITokenService
{
    LoginResponse CreateToken(string username, string role);
}