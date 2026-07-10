using StockApp.Data;
using StockApp.Models.Entities;

namespace StockApp.Repositories;

public class ProductRepository
{
    private readonly LiteDbContext _ctx;

    public ProductRepository(LiteDbContext ctx) => _ctx = ctx;

    public IEnumerable<Product> GetAll(int? companyId = null)
    {
        var query = _ctx.Products.Query();
        if (companyId.HasValue)
            query = query.Where(p => p.CompanyId == companyId.Value);
        return query.ToList();
    }

    public IEnumerable<Product> Search(string term, int? companyId = null)
    {
        var t = term.ToLower();
        var query = _ctx.Products.Query();
        if (companyId.HasValue)
            query = query.Where(p => p.CompanyId == companyId.Value);
        return query
            .ToList()
            .Where(p => p.Sku.ToLower().Contains(t) || p.Name.ToLower().Contains(t));
    }

    public Product? GetById(int id) => _ctx.Products.FindById(id);

    public Product Insert(Product product)
    {
        _ctx.Products.Insert(product);
        return product;
    }

    public bool Update(Product product) => _ctx.Products.Update(product);

    public bool Delete(int id) => _ctx.Products.Delete(id);

    public bool SkuExists(string sku, int? companyId = null, int? excludeId = null)
    {
        var query = _ctx.Products.Query().Where(p => p.Sku == sku);
        if (companyId.HasValue)
            query = query.Where(p => p.CompanyId == companyId.Value);

        var products = query.ToList();
        if (excludeId.HasValue)
            return products.Any(p => p.Id != excludeId.Value);

        return products.Any();
    }

    public IEnumerable<Product> GetLowStock(int? companyId = null)
    {
        var query = _ctx.Products.Query();
        if (companyId.HasValue)
            query = query.Where(p => p.CompanyId == companyId.Value);
        return query.Where(p => p.Quantity <= p.ReorderLevel).ToList();
    }
}
