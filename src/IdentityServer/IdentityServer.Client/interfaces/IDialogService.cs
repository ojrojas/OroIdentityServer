using IdentityServer.Client.Services;

namespace IdentityServer.Client.Interfaces;

public interface IDialogService
{
    event Action? OnChange;

    DialogRequest? Current { get; }

    /// <summary>Shows a confirmation dialog; resolves true when confirmed.</summary>
    Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText,
        string cancelText,
        bool danger = false);

    void Resolve(bool confirmed);
}
