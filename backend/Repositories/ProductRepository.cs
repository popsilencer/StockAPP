using LiteDB;
using StockApp.Data;
using StockApp.Models.Entities;

namespace StockApp.Repositories;

public class ProductRepository
{
    private readonly LiteDbContext _ctx;

    public ProductRepository(LiteDbContext ctx) => _ctx = ctx;

    public IEnumerable<Product> GetAll() => _ctx.Products.FindAll();

    public IEnumerable<Product> Search(string term)
    {
        var t = term.ToLower();
        return _ctx.Products.Find(p =>
            p.Sku.ToLower().Contains(t) || p.Name.ToLower().Contains(t));
    }

    public Product? GetById(int id) => _ctx.Products.FindById(id);

    public void Insert(Product product) => _ctx.Products.Insert(product);

    public bool Update(Product product) => _ctx.Products.Update(product);

    public bool Delete(int id) => _ctx.Products.Delete(id);

    public bool SkuExists(string sku, int? excludeId = null)
    {
        var p = _ctx.Products.FindOne(p => p.Sku == sku);
        return p != null && p.Id != excludeId;
    }

    public IEnumerable<Product> GetLowStock()
        => _ctx.Products.Find(p => p.Quantity <= p.ReorderLevel);
}
