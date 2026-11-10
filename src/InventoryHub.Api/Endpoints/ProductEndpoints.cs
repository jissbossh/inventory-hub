using InventoryHub.Api.Dtos;
using InventoryHub.Api.Services;

namespace InventoryHub.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (int page, int pageSize, string? search, int? categoryId,
            IProductService svc, CancellationToken ct) =>
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var data = await svc.GetPagedAsync(page, pageSize, search, categoryId, ct);
            return Results.Ok(ApiResponse<PagedResult<ProductDto>>.Ok(data));
        });

        group.MapGet("/{id:int}", async (int id, IProductService svc, CancellationToken ct) =>
        {
            var dto = await svc.GetByIdAsync(id, ct);
            return dto is null
                ? Results.NotFound(ApiResponse<ProductDto>.Fail("Product not found"))
                : Results.Ok(ApiResponse<ProductDto>.Ok(dto));
        });

        group.MapGet("/categories", async (IProductService svc, CancellationToken ct) =>
        {
            var data = await svc.GetCategoriesAsync(ct);
            return Results.Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(data));
        });

        group.MapPost("/{id:int}/stock", async (int id, StockChangeRequest req,
            IProductService svc, CancellationToken ct) =>
        {
            try
            {
                var updated = await svc.UpdateStockAsync(id, req.Delta, req.Reason, ct);
                return Results.Ok(ApiResponse<ProductDto>.Ok(updated));
            }
            catch (KeyNotFoundException ex) { return Results.NotFound(ApiResponse<ProductDto>.Fail(ex.Message)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(ApiResponse<ProductDto>.Fail(ex.Message)); }
        });

        return app;
    }
}

public record StockChangeRequest(int Delta, string? Reason);