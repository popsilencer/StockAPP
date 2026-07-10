using StockApp.Models.Dtos;
using StockApp.Models.Entities;
using StockApp.Repositories;

namespace StockApp.Services;

public class ProductService
{
    private readonly ProductRepository _repo;

    public ProductService(ProductRepository repo) => _repo = repo;

    public IEnumerable<Product> GetAll(string? search = null)
    {
        if (string.IsNullOrWhiteSpace(search))
            return _repo.GetAll();
        return _repo.Search(search);
    }

    public Product? GetById(int id) => _repo.GetById(id);

    public Product Create(ProductDto dto)
    {
        if (_repo.SkuExists(dto.Sku))
            throw new InvalidOperationException($"SKU '{dto.Sku}' already exists");

        var product = new Product
        {
            Sku = dto.Sku,
            Name = dto.Name,
            Description = dto.Description,
            Unit = dto.Unit,
            Quantity = dto.Quantity,
            ReorderLevel = dto.ReorderLevel
        };
        _repo.Insert(product);
        return product;
    }

    public Product Update(int id, ProductDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null)
            throw new KeyNotFoundException($"Product {id} not found");

        if (_repo.SkuExists(dto.Sku, excludeId: id))
            throw new InvalidOperationException($"SKU '{dto.Sku}' already exists");

        existing.Sku = dto.Sku;
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Unit = dto.Unit;
        existing.Quantity = dto.Quantity;
        existing.ReorderLevel = dto.ReorderLevel;
        _repo.Update(existing);
        return existing;
    }

    public void Delete(int id)
    {
        if (_repo.GetById(id) == null)
            throw new KeyNotFoundException($"Product {id} not found");
        _repo.Delete(id);
    }

    public IEnumerable<Product> GetLowStock() => _repo.GetLowStock();
}
