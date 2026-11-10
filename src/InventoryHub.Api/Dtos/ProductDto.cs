namespace InventoryHub.Api.Dtos;

public record ProductDto(
    int Id,
    string Name,
    string Sku,
    decimal Price,
    int Stock,
    int CategoryId,
    string CategoryName,
    bool IsActive,
    DateTime UpdatedUtc);