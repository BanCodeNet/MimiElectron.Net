using System;
using System.Threading.Tasks;

namespace MiniElectron.Core
{
    public static class Notification
    {
        public static Task<IpcMessage> NotificationIsSupported(this IpcBridge ipcBridge) => ipcBridge.Send("Notification.isSupported", waitResponse: true);
    }
}