using System;
using OroIdentity.Web.Client.Interfaces;

namespace OroIdentity.Web.Client.Services
{
    // Adapter that maps the app's IToastService to the Fluent UI ToastService
    public class FluentToastAdapter : IToastService
    {
        private readonly Microsoft.FluentUI.AspNetCore.Components.ToastService _fluentToast;

        public event Action<ToastMessage>? OnShow;

        public FluentToastAdapter(Microsoft.FluentUI.AspNetCore.Components.ToastService fluentToast)
        {
            _fluentToast = fluentToast ?? throw new ArgumentNullException(nameof(fluentToast));
        }

        public void ShowSuccess(string message, int durationMs = 5000)
        {
            _fluentToast.ShowSuccess(message, durationMs, null, default);
            OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), message, ToastLevel.Success, durationMs));
        }

        public void ShowError(string message, int durationMs = 5000)
        {
            _fluentToast.ShowError(message, durationMs, null, default);
            OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), message, ToastLevel.Error, durationMs));
        }

        public void ShowInfo(string message, int durationMs = 5000)
        {
            _fluentToast.ShowInfo(message, durationMs, null, default);
            OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), message, ToastLevel.Info, durationMs));
        }

        public void ShowWarning(string message, int durationMs = 5000)
        {
            _fluentToast.ShowWarning(message, durationMs, null, default);
            OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), message, ToastLevel.Warning, durationMs));
        }
    }
}
