import { Notification, dialog } from 'electron'
import { WebSocket } from 'ws'
import * as fs from 'fs'
import * as net from 'net'

class Bridge {
    private client: WebSocket

    constructor() { }

    isConnectted(): boolean {
        if (this.client == null || this.client.readyState != WebSocket.OPEN) return false
        return true
    }

    connect(url: string): void {
        this.client = new WebSocket(url)
        this.client.on('open', this.#onOpen)
        this.client.on('error', this.#onError)
    }

    #onOpen(): void {
        this.client.send('Register')
    }

    #onError(err: Error): void {
        console.log('XXXXX =====> ' + err.message)
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