using Xunit;
using StockApp.Data;
using StockApp.Models.Dtos;
using StockApp.Models.Entities;
using StockApp.Repositories;
using StockApp.Services;

namespace StockApp.Tests.Services;

public class StockServiceTests : IDisposable
{
    private readonly LiteDbContext _ctx;
    private readonly StockService _service;
    private readonly ProductRepository _productRepo;
    private readonly string _tempFile;

    public StockServiceTests()
    {
        _tempFile = Path.GetTempFileName();
        _ctx = new LiteDbContext(_tempFile);
        _productRepo = new ProductRepository(_ctx);
        var movementRepo = new MovementRepository(_ctx);
        _service = new StockService(_productRepo, movementRepo, _ctx);
    }

    [Fact]
    public void AdjustStock_In_IncreasesQuantity()
    {
        var p = new Product { Sku = "P1", Name = "P", Unit = "pcs", Quantity = 10, ReorderLevel = 5 };
        _productRepo.Insert(p);
        var req = new MovementRequest { Type = MovementType.In, Quantity = 3 };
        var m = _service.AdjustStock(p.Id, req);
        Assert.Equal(MovementType.In, m.Type);
        Assert.Equal(13, _productRepo.GetById(p.Id)!.Quantity);
    }

    [Fact]
    public void AdjustStock_Out_DecreasesQuantity()
    {
        var p = new Product { Sku = "P2", Name = "P", Unit = "pcs", Quantity = 10, ReorderLevel = 5 };
        _productRepo.Insert(p);
        var req = new MovementRequest { Type = MovementType.Out, Quantity = 3 };
        _service.AdjustStock(p.Id, req);
        Assert.Equal(7, _productRepo.GetById(p.Id)!.Quantity);
    }

    [Fact]
    public void AdjustStock_OutExceeds_Throws()
    {
        var p = new Product { Sku = "P3", Name = "P", Unit = "pcs", Quantity = 2, ReorderLevel = 5 };
        _productRepo.Insert(p);
        var req = new MovementRequest { Type = MovementType.Out, Quantity = 10 };
        Assert.Throws<InvalidOperationException>(() => _service.AdjustStock(p.Id, req));
    }

    [Fact]
    public void AdjustStock_NotFound_Throws()
    {
        var req = new MovementRequest { Type = MovementType.In, Quantity = 1 };
        Assert.Throws<KeyNotFoundException>(() => _service.AdjustStock(999, req));
    }

    public void Dispose()
    {
        _ctx.Dispose();
        try { File.Delete(_tempFile); } catch { }
    }
}
