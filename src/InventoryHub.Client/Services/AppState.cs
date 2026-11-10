namespace InventoryHub.Client.Services;

public class AppState
{
    public event Action? OnChange;
    public string? Search { get; private set; }
    public int? SelectedCategoryId { get; private set; }

    public void SetFilters(string? search, int? categoryId)
    {
        Search = search;
        SelectedCategoryId = categoryId;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}