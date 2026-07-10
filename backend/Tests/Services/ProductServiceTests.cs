using Xunit;
using StockApp.Data;
using StockApp.Models.Dtos;
using StockApp.Models.Entities;
using StockApp.Repositories;
using StockApp.Services;

namespace StockApp.Tests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly LiteDbContext _ctx;
    private readonly ProductService _service;
    private readonly string _tempFile;

    public ProductServiceTests()
    {
        _tempFile = Path.GetTempFileName();
        _ctx = new LiteDbContext(_tempFile);
        var repo = new ProductRepository(_ctx);
        _service = new ProductService(repo);
    }

    [Fact]
    public void Create_ReturnsProduct()
    {
        var dto = new ProductDto { Sku = "P001", Name = "Widget", Unit = "pcs", Quantity = 10, ReorderLevel = 5 };
        var result = _service.Create(dto);
        Assert.Equal("P001", result.Sku);
        Assert.Equal(10, result.Quantity);
    }

    [Fact]
    public void Create_DuplicateSku_Throws()
    {
        _service.Create(new ProductDto { Sku = "P001", Name = "A", Unit = "pcs" });
        Assert.Throws<InvalidOperationException>(() =>
            _service.Create(new ProductDto { Sku = "P001", Name = "B", Unit = "pcs" }));
    }

    [Fact]
    public void Update_NotFound_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            _service.Update(999, new ProductDto { Sku = "X", Name = "X", Unit = "pcs" }));
    }

    [Fact]
    public void Delete_RemovesProduct()
    {
        var p = _service.Create(new ProductDto { Sku = "P002", Name = "Test", Unit = "pcs" });
        _service.Delete(p.Id);
        Assert.Null(_service.GetById(p.Id));
    }

    [Fact]
    public void GetLowStock_ReturnsOnlyLowStock()
    {
        _service.Create(new ProductDto { Sku = "LOW", Name = "Low", Unit = "pcs", Quantity = 2, ReorderLevel = 5 });
        _service.Create(new ProductDto { Sku = "OK", Name = "OK", Unit = "pcs", Quantity = 100, ReorderLevel = 5 });
        var low = _service.GetLowStock().ToList();
        Assert.Single(low);
        Assert.Equal("LOW", low[0].Sku);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        try { File.Delete(_tempFile); } catch { }
    }
}
