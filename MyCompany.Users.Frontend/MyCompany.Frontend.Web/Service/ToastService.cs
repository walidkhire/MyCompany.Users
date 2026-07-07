using System;
using System.Collections.Generic;
using MyCompany.Frontend.Web.Models;

namespace MyCompany.Frontend.Web.Services
{
    public class ToastService
    {
        // Liste active des notifications à l'écran
        public List<ToastMessage> Toasts { get; } = new();

        // Événement déclenché à chaque ajout/suppression pour rafraîchir l'UI
        public event Action? OnToastChanged;

        public void ShowSuccess(string message) => AddToast(message, ToastType.Success);
        public void ShowError(string message) => AddToast(message, ToastType.Danger);
        public void ShowInfo(string message) => AddToast(message, ToastType.Info);
        public void ShowWarning(string message) => AddToast(message, ToastType.Warning);

        private void AddToast(string message, ToastType type)
        {
            var toast = new ToastMessage { Message = message, Type = type };
            Toasts.Add(toast);
            NotifyStateChanged();

            // Auto-destruction du toast après 5 secondes
            _ = RemoveToastAfterDelay(toast, 5000);
        }

        public void RemoveToast(ToastMessage toast)
        {
            if (Toasts.Contains(toast))
            {
                Toasts.Remove(toast);
                NotifyStateChanged();
            }
        }

        private async Task RemoveToastAfterDelay(ToastMessage toast, int delayMilliseconds)
        {
            await Task.Delay(delayMilliseconds);
            RemoveToast(toast);
        }

        private void NotifyStateChanged() => OnToastChanged?.Invoke();
    }
}