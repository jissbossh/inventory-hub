using System.Net.Http.Json;
using InventoryHub.Client.Models;

namespace InventoryHub.Client.Services;

public class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _http;

    public ProductApiClient(HttpClient http) => _http = http;

    public async Task<PagedResult<ProductDto>?> GetProductsAsync(int page, int pageSize, string? search, int? categoryId, CancellationToken ct = default)
    {
        var url = $"api/products?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (categoryId is > 0) url += $"&categoryId={categoryId}";

        var response = await _http.GetFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>(url, ct);
        return response?.Data;
    }

    public async Task<ProductDto?> GetProductAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ProductDto>>($"api/products/{id}", ct);
        return response?.Data;
    }

    public async Task<List<CategoryDto>?> GetCategoriesAsync(CancellationToken ct = default)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>("api/products/categories", ct);
        return response?.Data;
    }

    public async Task<ProductDto?> UpdateStockAsync(int id, int delta, string? reason, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"api/products/{id}/stock", new { delta, reason }, ct);
        if (!resp.IsSuccessStatusCode) return null;
        var payload = await resp.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(cancellationToken: ct);
        return payload?.Data;
    }
}