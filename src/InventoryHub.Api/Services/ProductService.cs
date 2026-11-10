using InventoryHub.Api.Data;
using InventoryHub.Api.Dtos;
using InventoryHub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InventoryHub.Api.Services;

public class ProductService : IProductService
{
    private readonly InventoryDbContext _db;
    private readonly ICacheService _cache;
    private readonly IConfiguration _cfg;

    public ProductService(InventoryDbContext db, ICacheService cache, IConfiguration cfg)
    {
        _db = db; _cache = cache; _cfg = cfg;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, string? search, int? categoryId, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var cacheKey = $"products:p{page}:s{pageSize}:q{search ?? "-"}:c{categoryId?.ToString() ?? "-"}";
        if (_cache.TryGet<PagedResult<ProductDto>>(cacheKey, out var cached) && cached is not null)
            return cached;

        var query = _db.Products.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%") || EF.Functions.Like(p.Sku, $"%{search}%"));

        if (categoryId is > 0)
            query = query.Where(p => p.CategoryId == categoryId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto(
                p.Id, p.Name, p.Sku, p.Price, p.Stock,
                p.CategoryId,
                p.Category != null ? p.Category.Name : "",
                p.IsActive, p.UpdatedUtc))
            .ToListAsync(ct);

        var result = new PagedResult<ProductDto>(items, page, pageSize, total);
        var seconds = _cfg.GetValue("Cache:ProductsSeconds", 30);
        _cache.Set(cacheKey, result, TimeSpan.FromSeconds(seconds));
        return result;
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var key = $"product:{id}";
        if (_cache.TryGet<ProductDto>(key, out var cached) && cached is not null) return cached;

        var dto = await _db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto(
                p.Id, p.Name, p.Sku, p.Price, p.Stock,
                p.CategoryId, p.Category != null ? p.Category.Name : "",
                p.IsActive, p.UpdatedUtc))
            .FirstOrDefaultAsync(ct);

        if (dto is not null) _cache.Set(key, dto, TimeSpan.FromSeconds(30));
        return dto;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct)
    {
        const string key = "categories:all";
        if (_cache.TryGet<IReadOnlyList<CategoryDto>>(key, out var cached) && cached is not null) return cached;

        var list = await _db.Categories.AsNoTracking()
            .Select(c => new CategoryDto(c.Id, c.Name, c.Products.Count))
            .ToListAsync(ct);

        _cache.Set(key, list, TimeSpan.FromSeconds(300));
        return list;
    }

    public async Task<ProductDto> UpdateStockAsync(int id, int delta, string? reason, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        var newStock = product.Stock + delta;
        if (newStock < 0) throw new InvalidOperationException("Stock cannot be negative");

        product.Stock = newStock;
        product.UpdatedUtc = DateTime.UtcNow;
        _db.StockMovements.Add(new StockMovement
        {
            ProductId = id,
            Type = delta >= 0 ? MovementType.In : MovementType.Out,
            Quantity = Math.Abs(delta),
            Reason = reason
        });

        await _db.SaveChangesAsync(ct);

        _cache.Remove($"product:{id}");
        _cache.RemoveByPrefix("products:");

        return new ProductDto(product.Id, product.Name, product.Sku, product.Price,
            product.Stock, product.CategoryId, "", product.IsActive, product.UpdatedUtc);
    }
}