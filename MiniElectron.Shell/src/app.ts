import { app, BrowserWindow, dialog } from 'electron';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import { spawn } from 'child_process';
import { Bridge } from './bridge';

const ipcFile = '.shellIpc'
const corePath = path.join(__dirname, 'extraResources', 'core/MiniElectron.Core')
console.debug(corePath)
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
    const ipcPath = path.join(os.tmpdir(), ipcFile)
    bridge = new Bridge(ipcPath)
    if (!fs.existsSync(corePath)) return;
    const core = spawn(corePath, [ipcPath, '6001'])
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