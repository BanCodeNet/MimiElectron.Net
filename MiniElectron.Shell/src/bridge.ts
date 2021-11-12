import { Notification } from 'electron';
import * as fs from 'fs';
import * as net from 'net';

class Bridge {
    private ipcServer: net.Server;

    constructor(ipcPath: string) {
        if (fs.existsSync(ipcPath)) {
            fs.unlinkSync(ipcPath)
        }
        this.ipcServer = net.createServer(socket => {
            socket.on('data', data => this.#onReceive(socket, data))
        })
        this.ipcServer.listen(ipcPath)
    }

    #onReceive(socket: net.Socket, data: Buffer): void {
        console.debug('msg ===> ' + data.toString())
        const json = JSON.parse(data.toString())
        console.warn(json)
        let isCallback = false
        switch (json.topic) {
            case "Notification.isSupported":
                json.body = Notification.isSupported()
                isCallback = true
                break;
        }
        if (isCallback) this.#send(socket, Buffer.from(JSON.stringify(json)))
    }

    #send(socket: net.Socket, data: Buffer): void {
        socket.write(Buffer.concat([data, Buffer.from('\r\n')]))
    }
}

export { Bridge }