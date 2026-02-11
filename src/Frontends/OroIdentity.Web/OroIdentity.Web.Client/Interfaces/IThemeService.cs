using System.Threading.Tasks;

namespace OroIdentity.Web.Client.Interfaces;

public interface IThemeService
{
    Task InitializeAsync();
    Task SetThemeAsync(string theme);
    Task<string?> GetThemeAsync();
    Task ToggleThemeAsync();
}
