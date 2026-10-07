using EmployeeMGR.Auth;
using EmployeeMGR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeMGR.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        if (request.UserName == "admin" && request.Password == "test123")
        {
            return Ok(
                _tokenService.CreateToken(
                    request.UserName,
                    "Admin"));
        }

        if (request.UserName == "user"
            && request.Password == "test123")
        {
            return Ok(
                _tokenService.CreateToken(
                    request.UserName,
                    "User"));
        }

        return Unauthorized();
    }
}