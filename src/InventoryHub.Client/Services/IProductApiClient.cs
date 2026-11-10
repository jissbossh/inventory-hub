using InventoryHub.Client.Models;

namespace InventoryHub.Client.Services;

public interface IProductApiClient
{
    Task<PagedResult<ProductDto>?> GetProductsAsync(int page, int pageSize, string? search, int? categoryId, CancellationToken ct = default);
    Task<ProductDto?> GetProductAsync(int id, CancellationToken ct = default);
    Task<List<CategoryDto>?> GetCategoriesAsync(CancellationToken ct = default);
    Task<ProductDto?> UpdateStockAsync(int id, int delta, string? reason, CancellationToken ct = default);
}