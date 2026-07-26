using IdentityServer.Client.Services;

namespace IdentityServer.Client.Interfaces;

public interface IToastService
{
    event Action? OnChange;

    IReadOnlyList<ToastMessage> Toasts { get; }

    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInfo(string message);

    void Remove(Guid id);
}
