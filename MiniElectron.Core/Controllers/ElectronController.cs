using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MiniElectron.Core
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("electron")]
    public sealed class ElectronController : Controller
    {
        private readonly IpcBridge _ipcBridge;

        public ElectronController(IpcBridge ipcBridge)
        {
            _ipcBridge = ipcBridge;
        }

        [HttpGet("notificationIsSupported")]
        public async Task<object> NotificationIsSupported()
        {
            return await _ipcBridge.NotificationIsSupported();
        }
    }
}