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
        return _ctx.Products.Query()
            .Where(p => p.Sku.ToLower().Contains(t) || p.Name.ToLower().Contains(t))
            .ToList();
    }

    public Product? GetById(int id) => _ctx.Products.FindById(id);

    public Product Insert(Product product)
    {
        _ctx.Products.Insert(product);
        return product;
    }

    public bool Update(Product product) => _ctx.Products.Update(product);

    public bool Delete(int id) => _ctx.Products.Delete(id);

    public bool SkuExists(string sku, int? excludeId = null)
    {
        var products = _ctx.Products.Query()
            .Where(p => p.Sku == sku)
            .ToList();

        if (excludeId.HasValue)
            return products.Any(p => p.Id != excludeId.Value);

        return products.Any();
    }

    public IEnumerable<Product> GetLowStock()
        => _ctx.Products.Query()
            .Where(p => p.Quantity <= p.ReorderLevel)
            .ToList();
}
