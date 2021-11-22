import { Notification, dialog } from 'electron'
import { WebSocket, RawData } from 'ws'
import * as fs from 'fs'
import * as net from 'net'

class Message {
    requestId: string
    topic: string
    body: any
    isCallback: boolean
}

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
        console.log('core to shell =====> ' + data.toString())
        let message: any
        let callback: any
        try {
            message = JSON.parse(data.toString())
            if (message == null) return
            switch (message.topic) {
                case "Callback":
                    break;
                case "Notification.isSupported":
                    callback = Notification.isSupported()
                    break;
                case "Notification.show":
                    let notification = new Notification(message.body)
                    notification.show();
                    break;
                case "dialog.showMessageBoxSync":
                    callback = dialog.showMessageBoxSync(message.body)
                    break;
                default:
                    break;
            }
        } catch (err) {
            callback = err.message
        } finally {
            if (message.isCallback) {
                const callbackMessage = new Message();
                callbackMessage.requestId = message.requestId
                callbackMessage.topic = 'Callback'
                callbackMessage.body = callback
                callbackMessage.isCallback = false
                this.send(JSON.stringify(callbackMessage))
            }
        }
    }

    #send(socket: net.Socket, data: Buffer): void {
        socket.write(Buffer.concat([data, Buffer.from('\r\n')]))
    }
}

export { Bridge }