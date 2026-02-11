using Microsoft.JSInterop;
using OroIdentity.Web.Client.Interfaces;

namespace OroIdentity.Web.Client.Services;

public sealed class ThemeService(IJSRuntime js) : IThemeService
{
    private const string THEME_KEY = "oro-theme";

    public async Task InitializeAsync()
    {
        var theme = await GetThemeAsync() ?? "light";
        await ApplyThemeAsync(theme);
    }

    public Task<string?> GetThemeAsync()
        => js.InvokeAsync<string?>("localStorage.getItem", THEME_KEY).AsTask();

    public async Task SetThemeAsync(string theme)
    {
        await js.InvokeVoidAsync("localStorage.setItem", THEME_KEY, theme);
        await ApplyThemeAsync(theme);
    }

    public async Task ToggleThemeAsync()
    {
        var current = await GetThemeAsync() ?? "light";
        var next = current == "dark" ? "light" : "dark";
        await SetThemeAsync(next);
    }

    private Task ApplyThemeAsync(string theme)
    {
        // Use a small eval to toggle the class on the root element so we don't need extra JS files
        var script = $"(function(){{ var el = document.documentElement; el.classList.remove('dark-theme'); if('{theme}' === 'dark') el.classList.add('dark-theme'); }})()";
        return js.InvokeVoidAsync("eval", script).AsTask();
    }
}
