using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace OroIdentity.Frontends.Services;

public interface INavigationHistoryService
{
    IReadOnlyList<BreadcrumbItem> History { get; }

    event Action? OnChanged;

    void Dispose();
}

public sealed class NavigationHistoryService : IDisposable, INavigationHistoryService
{
    private readonly NavigationManager _navigationManager;
    private readonly List<BreadcrumbItem> _history = [];
    public IReadOnlyList<BreadcrumbItem> History => _history;

    public event Action? OnChanged;

    public NavigationHistoryService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += HandleLocationChanged;
        AddCurrent();
    }

    private void AddCurrent()
    {
        Add(_navigationManager.Uri);
    }

    private void Add(string uri)
    {
        var relative = _navigationManager.ToBaseRelativePath(uri);

        if (_history.LastOrDefault()?.Url == relative)
            return;

        _history.Add(new BreadcrumbItem
        {
            Url = "/" + relative,
            Title = BuildTitle(relative)
        });

        OnChanged?.Invoke();
    }

    private string BuildTitle(string route)
    {
        if (string.IsNullOrEmpty(route))
            return "Home";

        return route
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Last()
            .Replace("-", " ")
            .ToUpperInvariant();
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        Add(e.Location);
    }

    public void Dispose()
    {
        _navigationManager.LocationChanged -= HandleLocationChanged;
    }
}

public class BreadcrumbItem
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}