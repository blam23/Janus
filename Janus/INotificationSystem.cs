using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janus
{
    public static class NotificationSystem
    {
        public static INotificationSystem Default = new WindowsToast();
    }

    public interface INotificationSystem
    {
        void Push(NotifcationType type, string title, string message);
    }
}
