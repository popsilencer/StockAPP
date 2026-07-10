using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models.Dtos;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService) => _userService = userService;

    [HttpGet]
    public IActionResult GetAll() => Ok(_userService.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var u = _userService.GetById(id);
        return u == null ? NotFound() : Ok(u);
    }

    [HttpPost]
    public IActionResult Create([FromBody] UserCreateDto dto)
    {
        try
        {
            var user = _userService.Create(dto);
            return Ok(user);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UserUpdateDto dto)
    {
        try
        {
            var user = _userService.Update(id, dto);
            return Ok(user);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}/password")]
    public IActionResult ChangePassword(int id, [FromBody] ChangePasswordDto dto)
    {
        try
        {
            _userService.ChangePassword(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var ok = _userService.Delete(id);
        return ok ? NoContent() : NotFound();
    }
}
