namespace InventoryHub.Client.Models;

public record ProductDto(
    int Id, string Name, string Sku, decimal Price, int Stock,
    int CategoryId, string CategoryName, bool IsActive, DateTime UpdatedUtc);

public record CategoryDto(int Id, string Name, int ProductCount);

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalItems)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public record ApiResponse<T>(bool Success, T? Data, string? Error);