import { app, BrowserWindow, dialog } from 'electron'
import * as fs from 'fs'
import * as os from 'os'
import * as path from 'path'
import { spawn } from 'child_process'
import { Bridge } from './bridge'

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
            try {
                bridge.connect('ws://localhost:' + httpPort)
                await new Promise(resolve => setTimeout(resolve, 500))
            } catch (err) {
                console.warn(err)
            }
        }
    })
})

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

}

const onCoreStdErr = (data: Buffer) => {
    console.debug(data.toString())
}

const onCoreClose = (code: number, signal: NodeJS.Signals) => {

}

const createWindow = () => {
    const win = new BrowserWindow({
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
}