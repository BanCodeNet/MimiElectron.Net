using System;
using System.Threading.Tasks;

namespace MiniElectron.Core
{
    public static class Notification
    {
        /// <summary>
        /// 当前系统是否支持桌面通知
        /// </summary>
        /// <param name="ipcBridge"></param>
        /// <returns></returns>
        public static Task<IpcMessage> NotificationIsSupported(this IpcBridge ipcBridge) => ipcBridge.Send("Notification.isSupported", waitResponse: true);

        public sealed record NotificationShowOption
        {
            public string title { get; init; }
            public string subtitle { get; init; }
            public string body { get; init; }
            public string silent { get; init; }
            public string icon { get; init; }
            public bool hasReply { get; init; }
            public string timeoutType { get; init; }
            public string replyPlaceholder { get; init; }
            public string sound { get; init; }
            public string urgency { get; init; }
            public string closeButtonText { get; init; }
            public string toastXml { get; init; }
        }

        /// <summary>
        /// 即时向用户展示桌面通知
        /// </summary>
        /// <param name="ipcBridge"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static Task NotificationShow(this IpcBridge ipcBridge, NotificationShowOption option = null) => ipcBridge.Send("Notification.show", option);
    }
}