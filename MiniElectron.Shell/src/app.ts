import { app, BrowserWindow, dialog, Menu, Tray, nativeTheme } from 'electron';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import { spawn } from 'child_process';
import { Bridge } from './bridge';
//import * as ws from 'ws';
import { WebSocket } from 'ws'

const httpPort: number = 6001
let corePath: string
switch (os.platform()) {
    case 'win32':
        corePath = path.join(__dirname, 'extraResources', 'core/MiniElectron.Core.exe')
        break
    default:
        corePath = path.join(__dirname, 'extraResources', 'core/MiniElectron.Core')
        break
}
let bridge: Bridge;

app.on("ready", () => {
    startCore()
    createWindow()
    bridge = new Bridge()
    new Promise(async resolve => {
        while (!bridge.isConnectted()) {
            bridge.connect('ws://localhost:' + httpPort)
            await new Promise(resolve => setTimeout(resolve, 500))
        }
    })
})

const themeMenu = () => {
    const template =
        [
            {
                label: 'ThemeSwitch', submenu: [
                    {
                        label: 'dark',
                        click: () => {
                            let allWins = BrowserWindow.getAllWindows();
                            if (allWins != null && allWins.length > 0) {
                                nativeTheme.themeSource = 'dark';
                                //allWins.forEach(win => win.webContents.send('change_theme', 'dark'));
                                //ipcRenderer on change_theme
                            }
                        }
                    },
                    {
                        label: 'white',
                        click: () => {
                            let allWins = BrowserWindow.getAllWindows();
                            if (allWins != null && allWins.length > 0) {
                                nativeTheme.themeSource = 'light';
                                //allWins.forEach(win => win.webContents.send('change_theme', 'white'));
                                //ipcRenderer on change_theme
                            }
                        }
                    }
                ]
            }
        ];
    const menu = Menu.buildFromTemplate(template)
    Menu.setApplicationMenu(menu)
}

app.on("window-all-closed", () => {
    if (process.platform != "darwin") {
        app.quit()
    }
})

app.on("activate", () => {
    if (BrowserWindow.getAllWindows.length == 0) {
        createWindow()
    }
})

let tray = null
app.whenReady().then(() => {
    tray = new Tray('desktopIcon.ico')
    const contextMenu = Menu.buildFromTemplate([
        {
            label: 'exit', type: 'normal', click: function () {
                app.quit();
            }
        }
    ])
    tray.setToolTip('My electron Application')
    tray.setContextMenu(contextMenu)
    tray.on('click', function () { win.show() })
    tray.on('double-click', function () { })

    // var wss = new ws("ws://121.40.165.18:8800");
    // var a = wss.readyState;
    // wss.onopen = function () {
    //     var aa = wss.readyState;
    //     console.log("open");
    //     wss.send("hello");
    // }
    // wss.onmessage = function (e) {
    //     console.log(e.data);
    // }
    // wss.onclose = function (e) {
    //     console.log("close");
    // }
    // wss.onerror = function (e) {
    //     console.log(e);
    // }
})

//     var win = BrowserWindow.getFocusedWindow()
//     win.on('close', (e) => {
//         if (!win.isFocused()) {
//             win = null
//         }
//         else {
//             e.preventDefault()
//             win.hide()
//         }
//     })


const startCore = () => {
    if (!fs.existsSync(corePath)) return;
    const core = spawn(corePath, [httpPort.toString()])
    core.stdout.on('data', onCoreStdOut)
    core.stderr.on('data', onCoreStdErr)
    core.on('close', onCoreClose)
    app.on('quit', () => {
        core.kill()
    })
}

const onCoreStdOut = (data: Buffer) => {
    console.debug(data.toString())
}

const onCoreStdErr = (data: Buffer) => {
    console.debug(data.toString())
}

const onCoreClose = (code: number, signal: NodeJS.Signals) => {

}

let win
const createWindow = () => {
    win = new BrowserWindow({
        width: 1366,
        height: 1024,
        show: false,
        webPreferences: {
            nodeIntegration: true,
            contextIsolation: false
        }
    })

    win.once('ready-to-show', () => {
        win.show()
        // win.webContents.openDevTools()
    })
    win.loadFile('index.html')
    win.on('closed', () => {
        win = null
    })

    themeMenu();
}