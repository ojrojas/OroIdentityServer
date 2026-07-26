using IdentityServer.Client.Interfaces;

namespace IdentityServer.Client.Services;

public enum ToastLevel { Success, Error, Warning, Info }

public sealed record ToastMessage(Guid Id, ToastLevel Level, string Message);

/// <summary>
/// Minimal replacement for FluentUI's IToastService. Components subscribe to
/// <see cref="OnChange"/>; <c>ToastHost</c> renders the queue.
/// </summary>
public sealed class ToastService : IToastService, IDisposable
{
    private readonly List<ToastMessage> _toasts = [];
    private readonly List<Timer> _timers = [];
    private readonly Lock _gate = new();

    public event Action? OnChange;

    public IReadOnlyList<ToastMessage> Toasts
    {
        get { lock (_gate) return [.. _toasts]; }
    }

    public void ShowSuccess(string message) => Show(ToastLevel.Success, message);
    public void ShowError(string message) => Show(ToastLevel.Error, message);
    public void ShowWarning(string message) => Show(ToastLevel.Warning, message);
    public void ShowInfo(string message) => Show(ToastLevel.Info, message);

    public void Show(ToastLevel level, string message, int durationMs = 4500)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        var toast = new ToastMessage(Guid.NewGuid(), level, message);

        lock (_gate)
        {
            _toasts.Add(toast);

            // Errors linger a little longer - they usually need reading.
            var delay = level == ToastLevel.Error ? durationMs + 2500 : durationMs;
            var timer = new Timer(_ => Remove(toast.Id), null, delay, Timeout.Infinite);
            _timers.Add(timer);
        }

        OnChange?.Invoke();
    }

    public void Remove(Guid id)
    {
        bool removed;
        lock (_gate) removed = _toasts.RemoveAll(t => t.Id == id) > 0;

        if (removed) OnChange?.Invoke();
    }

    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var timer in _timers) timer.Dispose();
            _timers.Clear();
            _toasts.Clear();
        }
    }
}
