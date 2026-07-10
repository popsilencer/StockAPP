using LiteDB;
using StockApp.Data;
using StockApp.Models.Entities;

namespace StockApp.Repositories;

public class MovementRepository
{
    private readonly LiteDbContext _ctx;

    public MovementRepository(LiteDbContext ctx) => _ctx = ctx;

    public IEnumerable<StockMovement> GetAll(int? productId = null)
    {
        if (productId.HasValue)
            return _ctx.Movements.Find(m => m.ProductId == productId.Value)
                .OrderByDescending(m => m.CreatedAt);
        return _ctx.Movements.FindAll().OrderByDescending(m => m.CreatedAt);
    }

    public void Insert(StockMovement movement) => _ctx.Movements.Insert(movement);
}
