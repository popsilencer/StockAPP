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
        products.EnsureIndex(p => p.Sku, unique: true);

        _db.GetCollection<StockMovement>("movements", BsonAutoId.Int32);
        _db.GetCollection<User>("users", BsonAutoId.Int32);
    }

    public ILiteCollection<Product> Products => _db.GetCollection<Product>("products");
    public ILiteCollection<StockMovement> Movements => _db.GetCollection<StockMovement>("movements");
    public ILiteCollection<User> Users => _db.GetCollection<User>("users");

    public void BeginTransaction() => _db.BeginTrans();
    public void CommitTransaction() => _db.Commit();
    public void RollbackTransaction() => _db.Rollback();

    public void Dispose() => _db.Dispose();
}
