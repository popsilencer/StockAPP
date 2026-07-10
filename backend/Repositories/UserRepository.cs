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

    public User? GetById(int id) => _ctx.Users.FindById(id);

    public IEnumerable<User> GetAll() => _ctx.Users.FindAll();

    public void Insert(User user) => _ctx.Users.Insert(user);

    public bool Update(User user) => _ctx.Users.Update(user);

    public bool Delete(int id) => _ctx.Users.Delete(id);

    public bool UsernameExists(string username, int? excludeId = null)
    {
        var users = _ctx.Users.Query()
            .Where(u => u.Username == username)
            .ToList();

        if (excludeId.HasValue)
            return users.Any(u => u.Id != excludeId.Value);

        return users.Any();
    }
}
