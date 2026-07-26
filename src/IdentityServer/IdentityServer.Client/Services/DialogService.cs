using IdentityServer.Client.Interfaces;

namespace IdentityServer.Client.Services;

public sealed record DialogRequest(
    string Title,
    string Message,
    string ConfirmText,
    string CancelText,
    bool Danger);

/// <summary>
/// Minimal replacement for FluentUI's IDialogService, limited to what the admin
/// pages actually need: an awaitable confirmation. <c>DialogHost</c> renders it.
/// </summary>
public sealed class DialogService : IDialogService
{
    private TaskCompletionSource<bool>? _pending;

    public event Action? OnChange;

    public DialogRequest? Current { get; private set; }

    public Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText,
        string cancelText,
        bool danger = false)
    {
        // A second prompt while one is open cancels the first rather than losing it.
        _pending?.TrySetResult(false);

        Current = new DialogRequest(title, message, confirmText, cancelText, danger);
        _pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        OnChange?.Invoke();
        return _pending.Task;
    }

    public void Resolve(bool confirmed)
    {
        var pending = _pending;

        Current = null;
        _pending = null;

        OnChange?.Invoke();
        pending?.TrySetResult(confirmed);
    }
}
