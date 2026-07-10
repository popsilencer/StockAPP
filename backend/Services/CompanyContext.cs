using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StockApp.Repositories;

namespace StockApp.Services;

/// <summary>
/// Resolves the current user's company scope.
/// Returns null for admin (see all), otherwise the user's CompanyId.
/// </summary>
public class CompanyContext
{
    private readonly UserRepository _userRepo;

    public CompanyContext(UserRepository userRepo) => _userRepo = userRepo;

    public int? Resolve(HttpContext http)
    {
        var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = http.User.FindFirst(ClaimTypes.Name)?.Value;

        if (username == "admin")
            return null; // admin sees everything

        if (userIdStr != null && int.TryParse(userIdStr, out int userId))
        {
            var user = _userRepo.GetById(userId);
            return user?.CompanyId;
        }

        return null;
    }
}
