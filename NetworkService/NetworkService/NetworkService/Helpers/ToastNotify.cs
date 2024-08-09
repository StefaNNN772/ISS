using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notification.Wpf;
using Notifications.Wpf;

namespace NetworkService.Helpers
{
    public static class ToastNotify
    {
        static readonly NotificationManager NotificationManager;
        static ToastNotify()
        {
            NotificationManager = new NotificationManager();
        }

        public static void RaiseToast(string mainText,string description,NotificationType type)
        {
            NotificationManager.Show(mainText, description, type, "MainNotificationArea");
        }
    }
}
