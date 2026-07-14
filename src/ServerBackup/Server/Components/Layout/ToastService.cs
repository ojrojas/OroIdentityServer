namespace OroIdentityServer.Server.Components.Layout;

public sealed class ToastService
{
    public event Action<string, string>? OnToast;

    public void Success(string message) => OnToast?.Invoke("success", message);
    public void Error(string message) => OnToast?.Invoke("error", message);
    public void Info(string message) => OnToast?.Invoke("info", message);
}
