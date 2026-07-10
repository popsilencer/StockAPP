using StockApp.Data;
using StockApp.Models.Entities;

namespace StockApp.Repositories;

public class WithdrawDetailRepository
{
    private readonly LiteDbContext _ctx;

    public WithdrawDetailRepository(LiteDbContext ctx) => _ctx = ctx;

    public List<WithdrawDetail> GetByWithdrawNo(string withdrawNo)
        => _ctx.WithdrawDetails.Query()
            .Where(d => d.WithdrawNo == withdrawNo)
            .ToList();

    public void InsertBulk(IEnumerable<WithdrawDetail> details)
        => _ctx.WithdrawDetails.InsertBulk(details);

    public int DeleteByWithdrawNo(string withdrawNo)
        => _ctx.WithdrawDetails.DeleteMany(d => d.WithdrawNo == withdrawNo);
}
