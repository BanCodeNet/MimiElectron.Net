import { Notification, dialog } from 'electron';
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
        let json: any
        let callback: any
        try {
            json = JSON.parse(data.toString())
            switch (json.topic) {
                case "Notification.isSupported":
                    callback = Notification.isSupported()
                    break;
                case "Notification.show":
                    let notification = new Notification(json.body)
                    notification.show();
                    break;
                case "dialog.showMessageBoxSync":
                    callback = dialog.showMessageBoxSync(json.body)
                    break;
            }
        } catch (err) {
            callback = err.message
        } finally {
            if (json.isCallback) this.#send(socket, Buffer.from(JSON.stringify(callback)))
        }
    }

    #send(socket: net.Socket, data: Buffer): void {
        socket.write(Buffer.concat([data, Buffer.from('\r\n')]))
    }
}

export { Bridge }