using LiteDB;
using StockApp.Models.Entities;

namespace StockApp.Data;

public class LiteDbContext : IDisposable
{
    private readonly LiteDatabase _db;

    public LiteDbContext(string dbPath)
    {
        var cs = new ConnectionString
        {
            Filename = dbPath,
            Connection = ConnectionType.Shared
        };
        _db = new LiteDatabase(cs);

        var products = _db.GetCollection<Product>("products", BsonAutoId.Int32);
        // Drop legacy unique index (name may be 'Sku' or 'sku' depending on version)
        products.DropIndex("Sku");
        products.DropIndex("sku");
        products.EnsureIndex(p => p.Sku);
        products.EnsureIndex(p => p.CompanyId);

        var movements = _db.GetCollection<StockMovement>("movements", BsonAutoId.Int32);
        movements.EnsureIndex(m => m.CompanyId);

        _db.GetCollection<User>("users", BsonAutoId.Int32);
        var withdraws = _db.GetCollection<Withdraw>("withdraws", BsonAutoId.Int32);
        withdraws.EnsureIndex(w => w.CompanyId);

        _db.GetCollection<WithdrawDetail>("withdrawdetails", BsonAutoId.Int32);

        var companies = _db.GetCollection<Company>("companies", BsonAutoId.Int32);
        companies.EnsureIndex(c => c.Tax, unique: true);

        _db.GetCollection<BsonDocument>("counters");
    }

    public LiteDatabase Database => _db;
    public ILiteCollection<Product> Products => _db.GetCollection<Product>("products");
    public ILiteCollection<StockMovement> Movements => _db.GetCollection<StockMovement>("movements");
    public ILiteCollection<User> Users => _db.GetCollection<User>("users");
    public ILiteCollection<Withdraw> Withdraws => _db.GetCollection<Withdraw>("withdraws");
    public ILiteCollection<WithdrawDetail> WithdrawDetails => _db.GetCollection<WithdrawDetail>("withdrawdetails");
    public ILiteCollection<Company> Companies => _db.GetCollection<Company>("companies");
    public ILiteCollection<BsonDocument> Counters => _db.GetCollection<BsonDocument>("counters");

    public void BeginTransaction() => _db.BeginTrans();
    public void CommitTransaction() => _db.Commit();
    public void RollbackTransaction() => _db.Rollback();

    public void Dispose() => _db.Dispose();
}
