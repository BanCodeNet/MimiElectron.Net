import { app, BrowserWindow, dialog } from 'electron';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import { spawn } from 'child_process';
import { Bridge } from './bridge';

const sockName = 'coreToShell'
let sockPath: string
const corePath = path.join(__dirname, 'extraResources', 'core/MiniElectron.Core')
switch (os.platform()) {
    case 'win32':
        sockPath = path.join('\\\\.\\pipe', sockName)
        break
    default:
        sockPath = path.join(os.tmpdir(), sockName)
        break
}
let bridge: Bridge;

app.on("ready", () => {
    createWindow()
    startCore()
})

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

const startCore = () => {
    if (sockName == null) return
    bridge = new Bridge(sockName)
    if (!fs.existsSync(corePath)) return;
    const core = spawn(corePath, [sockName, '6001'])
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