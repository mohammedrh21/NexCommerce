using System;
using System.Collections.Generic;

namespace Client.Services;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public ToastType Type { get; }
    public string Title { get; }
    public string Message { get; }
    public int DurationMs { get; }

    public ToastMessage(ToastType type, string message, string title = "", int durationMs = 5000)
    {
        Type = type;
        Message = message;
        Title = string.IsNullOrEmpty(title) ? DefaultTitle(type) : title;
        DurationMs = durationMs;
    }

    private static string DefaultTitle(ToastType type) => type switch
    {
        ToastType.Success => "Success",
        ToastType.Error => "Error",
        ToastType.Warning => "Warning",
        ToastType.Info => "Information",
        _ => ""
    };
}

public interface IToastService
{
    event Action OnToastsChanged;
    IReadOnlyList<ToastMessage> Toasts { get; }
    void Success(string message, string title = "", int durationMs = 5000);
    void Error(string message, string title = "", int durationMs = 5000);
    void Warning(string message, string title = "", int durationMs = 5000);
    void Info(string message, string title = "", int durationMs = 5000);
    void Remove(Guid id);
}

public class ToastService : IToastService
{
    private readonly List<ToastMessage> _toasts = new();
    
    public event Action? OnToastsChanged;
    public IReadOnlyList<ToastMessage> Toasts => _toasts.AsReadOnly();

    public void Success(string message, string title = "", int durationMs = 5000) =>
        AddToast(new ToastMessage(ToastType.Success, message, title, durationMs));

    public void Error(string message, string title = "", int durationMs = 5000) =>
        AddToast(new ToastMessage(ToastType.Error, message, title, durationMs));

    public void Warning(string message, string title = "", int durationMs = 5000) =>
        AddToast(new ToastMessage(ToastType.Warning, message, title, durationMs));

    public void Info(string message, string title = "", int durationMs = 5000) =>
        AddToast(new ToastMessage(ToastType.Info, message, title, durationMs));

    public void Remove(Guid id)
    {
        var toast = _toasts.Find(t => t.Id == id);
        if (toast != null)
        {
            _toasts.Remove(toast);
            NotifyStateChanged();
        }
    }

    private void AddToast(ToastMessage toast)
    {
        _toasts.Add(toast);
        NotifyStateChanged();
        
        // Auto-dismiss after duration
        if (toast.DurationMs > 0)
        {
            System.Threading.Tasks.Task.Delay(toast.DurationMs).ContinueWith(_ =>
            {
                Remove(toast.Id);
            });
        }
    }

    private void NotifyStateChanged() => OnToastsChanged?.Invoke();
}
