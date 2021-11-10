const { app, BrowserWindow, ipcMain } = require('electron')
const fs = require('fs')
const net = require('net')
const os = require('os');
const path = require('path');
const { spawn } = require('child_process');
const ipcFile = '.minielectronIpc'

app.on("ready", () => {
    createWindow()
    startCore()
})

function createWindow() {
    const win = new BrowserWindow({
        width: 1200,
        height: 800,
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
    // win.loadFile('index.html')
    win.loadURL('https://www.baidu.com');
}

function startCore() {
    const ipcPath = path.join(os.tmpdir(), ipcFile)
    const ipcServer = net.createServer(socket => {
        socket.on('data', onCoreReceive)
    })
    if (fs.existsSync(ipcPath)) {
        fs.unlinkSync(ipcPath)
    }
    ipcServer.listen(ipcPath)
    const core = spawn('core/MiniElectron.Core', [ipcPath])
    core.stdout.on('data', onCoreStdOut)
    core.stderr.on('data', onCoreStdErr)
    core.on('close', onCoreClose)
    app.on('quit', () => {
        core.kill()
    })
}

function onCoreStdOut(data) {

}

function onCoreStdErr(data) {

}

function onCoreClose(code, signal) {

}

function onCoreReceive(data) {
    console.debug("msg ===> " + data.toString())
}