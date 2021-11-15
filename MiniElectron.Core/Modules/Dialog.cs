using System;
using System.Threading.Tasks;

namespace MiniElectron.Core
{
    public static class Dialog
    {
        public sealed record ShowMessageBoxSyncOptions
        {
            public string message { get; init; }
            public string type { get; init; }
            public string[] buttons { get; init; }
            public int defaultId { get; init; }
            public string title { get; init; }
            public string detail { get; init; }
            public string icon { get; init; }
            public int textWidth { get; init; }
            public int cancelId { get; init; }
            public bool noLink { get; init; }
            public bool normalizeAccessKeys { get; init; }
        }

        public static Task<IpcMessage> DialogShowMessageBoxSync(this IpcBridge ipcBridge, ShowMessageBoxSyncOptions options) => ipcBridge.SendAsync("dialog.showMessageBoxSync", options, true);
    }
}