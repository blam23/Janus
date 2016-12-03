using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Janus
{
    class WindowsToast : INotificationSystem
    {
        private const string AppID = "Harris.Janus.Main";
        private readonly ToastNotifier _notifier = ToastNotificationManager.CreateToastNotifier(AppID);

        public void Push(NotifcationType type, string title, string message)
        {
            var toastXML = new XmlDocument();
            toastXML.LoadXml($@"<toast><visual><binding template='ToastGeneric'><text>{title}</text><text>{message}</text></binding></visual></toast>");

            var toast = new ToastNotification(toastXML) {ExpirationTime = DateTimeOffset.Now.AddHours(1)};
            _notifier.Show(toast);
        }
    }
}
