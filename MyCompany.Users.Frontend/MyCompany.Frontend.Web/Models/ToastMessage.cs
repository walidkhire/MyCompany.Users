using System;

namespace MyCompany.Frontend.Web.Models
{
    public class ToastMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; } = ToastType.Info;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum ToastType
    {
        Success,
        Danger,
        Info,
        Warning
    }
}