import { Notification, dialog } from 'electron'
import { WebSocket, RawData } from 'ws'
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
        this.client.on('close', this.#onClose)
        this.client.on('error', this.#onError)
        this.client.on('message', this.#onMessage)
    }

    #onOpen(this: WebSocket): void {
        this.send(JSON.stringify({ topic: 'Register' }))
    }

    #onClose(this: WebSocket, code: number, reason: Buffer): void {

    }

    #onError(this: WebSocket, err: Error): void {

    }

    #onMessage(this: WebSocket, data: RawData, isBinary: boolean): void {
        let json: any
        let callback: any
        try {
            json = JSON.parse(data.toString())
            if (json?.topic == null) throw new Error('未定义Topic')
            switch (json?.topic) {
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
                default:
                    throw new Error('未定义的Topic:' + json.topic)
            }
        } catch (err) {
            callback = err.message
        } finally {
            if (json?.isCallback) this.send(callback)
            console.error(callback)
        }
    }

    #send(socket: net.Socket, data: Buffer): void {
        socket.write(Buffer.concat([data, Buffer.from('\r\n')]))
    }
}

export { Bridge }