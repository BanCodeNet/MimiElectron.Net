import { Notification, dialog } from 'electron'
import { client as WebsocketClient } from 'websocket';
import * as fs from 'fs'
import * as net from 'net'

class Bridge {
    private ipcServer: net.Server;
    private client: WebsocketClient;

    constructor(httpPort: number) {
        setTimeout(() => {
            var connectUrl = 'ws://localhost:' + httpPort
            this.client = new WebsocketClient()
            this.client.on('connectFailed', function (error) {
                console.log('Connect Error: ' + error.toString());
            });
            this.client.connect(connectUrl)
        }, 3000);
        // this.ipcServer = net.createServer(socket => {
        //     socket.on('data', data => this.#onReceive(socket, data))
        // })
        // this.ipcServer.listen(ipcPath)
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