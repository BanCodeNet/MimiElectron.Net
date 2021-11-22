using System;
using System.Threading.Tasks;

namespace MiniElectron.Core
{
    public static class Dialog
    {
        public sealed record ShowMessageBoxOptions
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

        /// <summary>
        /// 显示一个消息框
        /// </summary>
        /// <param name="bridge"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Task<Message> DialogShowMessageBox(this Bridge bridge, ShowMessageBoxOptions options) => bridge.SendAsync("dialog.showMessageBoxSync", options, true, -1);
    }
}