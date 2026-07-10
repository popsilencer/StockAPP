using System.ComponentModel.DataAnnotations;

namespace StockApp.Models.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
}

public class UserCreateDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    public string Password { get; set; } = string.Empty;

    public int? CompanyId { get; set; }
}

public class UserUpdateDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    public int? CompanyId { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    [MinLength(4)]
    public string NewPassword { get; set; } = string.Empty;
}
