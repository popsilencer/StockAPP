using StockApp.Data;
using StockApp.Models.Entities;

namespace StockApp.Repositories;

public class UserRepository
{
    private readonly LiteDbContext _ctx;

    public UserRepository(LiteDbContext ctx) => _ctx = ctx;

    public User? GetByUsername(string username)
        => _ctx.Users.Query()
            .Where(u => u.Username == username)
            .FirstOrDefault();

    public void Insert(User user) => _ctx.Users.Insert(user);
}
