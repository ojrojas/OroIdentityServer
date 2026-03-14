using System;

namespace OroIdentity.Web.Client.Interfaces
{
    public enum ToastLevel
    {
        Success,
        Error,
        Info,
        Warning
    }

    public record ToastMessage(Guid Id, string Message, ToastLevel Level = ToastLevel.Info, int DurationMs = 5000);

    public interface IToastService
    {
        event Action<ToastMessage>? OnShow;
        void ShowSuccess(string message, int durationMs = 5000);
        void ShowError(string message, int durationMs = 5000);
        void ShowInfo(string message, int durationMs = 5000);
        void ShowWarning(string message, int durationMs = 5000);
    }
}
