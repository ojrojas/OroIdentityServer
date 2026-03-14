using System;
using OroIdentity.Web.Client.Interfaces;

namespace OroIdentity.Web.Client.Services
{
    public class ToastService : IToastService
    {
        public event Action<ToastMessage>? OnShow;

        public void ShowSuccess(string message, int durationMs = 5000) => Show(message, ToastLevel.Success, durationMs);
        public void ShowError(string message, int durationMs = 5000) => Show(message, ToastLevel.Error, durationMs);
        public void ShowInfo(string message, int durationMs = 5000) => Show(message, ToastLevel.Info, durationMs);
        public void ShowWarning(string message, int durationMs = 5000) => Show(message, ToastLevel.Warning, durationMs);

        private void Show(string message, ToastLevel level, int durationMs)
        {
            var toast = new ToastMessage(Guid.NewGuid(), message, level, durationMs);
            OnShow?.Invoke(toast);
        }
    }
}
