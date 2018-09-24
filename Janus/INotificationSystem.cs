
namespace Janus
{
    public static class NotificationSystem
    {
        public static INotificationSystem Default { get; } = new WindowsToast();
    }

    public interface INotificationSystem
    {
        void Push(NotifcationType type, string title, string message);
    }
}
