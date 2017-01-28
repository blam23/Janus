using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Janus
{
    /// <summary>
    /// Provides Windows 8+ Style Notifications
    /// May only work with Windows 10, unsure.
    /// </summary>
    internal class WindowsToast : INotificationSystem
    {
        // TODO: This is what gets displayed in the Notifcation Center on Win 10, need a way to make it display a proper name.
        private const string AppId = "Janus.Main";
        private readonly ToastNotifier _notifier = ToastNotificationManager.CreateToastNotifier(AppId);

        /// <summary>
        /// Sends out a basic Windows 8 style notification
        /// </summary>
        /// <param name="type">Icon to be shown</param>
        /// <param name="title">Title of Notification</param>
        /// <param name="message">Main message content (plaintext only)</param>
        public void Push(NotifcationType type, string title, string message)
        {
            var toastXml = new XmlDocument();
            toastXml.LoadXml($@"<toast><visual><binding template='ToastGeneric'><text>{title}</text><text>{message}</text></binding></visual></toast>");

            var toast = new ToastNotification(toastXml) {ExpirationTime = DateTimeOffset.Now.AddHours(1)};
            _notifier.Show(toast);
        }
    }
}
