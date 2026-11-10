using InventoryHub.Api.Dtos;

namespace InventoryHub.Api.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, string? search, int? categoryId, CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct);
    Task<ProductDto> UpdateStockAsync(int id, int delta, string? reason, CancellationToken ct);
}