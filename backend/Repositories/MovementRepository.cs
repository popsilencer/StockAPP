using StockApp.Models.Entities;
using StockApp.Data;

namespace StockApp.Repositories;

public class MovementRepository
{
    private readonly LiteDbContext _ctx;

    public MovementRepository(LiteDbContext ctx) => _ctx = ctx;

    public IEnumerable<StockMovement> GetAll(int? productId = null, int? companyId = null)
    {
        var query = _ctx.Movements.Query();
        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);
        if (companyId.HasValue)
            query = query.Where(m => m.CompanyId == companyId.Value);

        return query.OrderBy(m => m.CreatedAt)
            .ToList()
            .OrderByDescending(m => m.CreatedAt);
    }

    public void Insert(StockMovement movement) => _ctx.Movements.Insert(movement);
}
