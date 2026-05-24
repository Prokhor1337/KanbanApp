namespace KanbanApp.Web.Client.Services
{
    public enum ToastType { Success, Error, Info }

    public class ToastMessage
    {
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class ToastService
    {
        public event Action<ToastMessage>? OnShow;

        public void ShowSuccess(string message) =>
            OnShow?.Invoke(new ToastMessage { Message = message, Type = ToastType.Success });

        public void ShowError(string message) =>
            OnShow?.Invoke(new ToastMessage { Message = message, Type = ToastType.Error });

        public void ShowInfo(string message) =>
            OnShow?.Invoke(new ToastMessage { Message = message, Type = ToastType.Info });
    }
}
