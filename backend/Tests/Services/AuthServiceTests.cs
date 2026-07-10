using Xunit;
using Microsoft.Extensions.Configuration;
using StockApp.Data;
using StockApp.Models.Entities;
using StockApp.Repositories;
using StockApp.Services;

namespace StockApp.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly LiteDbContext _ctx;
    private readonly AuthService _service;
    private readonly string _tempFile;

    public AuthServiceTests()
    {
        _tempFile = Path.GetTempFileName();
        _ctx = new LiteDbContext(_tempFile);
        var userRepo = new UserRepository(_ctx);
        userRepo.Insert(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
        });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "stock-app-secret-key-must-be-at-least-32-chars!" },
                { "Jwt:Issuer", "StockApp" },
                { "Jwt:Audience", "StockApp" }
            })
            .Build();
        _service = new AuthService(userRepo, config);
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsToken()
    {
        var token = _service.Login("admin", "admin123");
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void Login_InvalidPassword_ReturnsNull()
    {
        var token = _service.Login("admin", "wrong");
        Assert.Null(token);
    }

    [Fact]
    public void Login_NonExistentUser_ReturnsNull()
    {
        var token = _service.Login("nobody", "pass");
        Assert.Null(token);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        try { File.Delete(_tempFile); } catch { }
    }
}
