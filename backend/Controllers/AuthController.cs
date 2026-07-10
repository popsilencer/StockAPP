using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models.Dtos;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var token = _authService.Login(request.Username, request.Password);
        if (token == null)
            return Unauthorized(new { message = "Invalid username or password" });
        return Ok(new LoginResponse { Token = token });
    }
}
