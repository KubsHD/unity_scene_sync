import * as WebSocket from "ws";

// todo: make this into a callback array
import { wsClientConnentedCallback } from "./main";

let server : WebSocket.Server;

// WebSockets only used to notify clients of changes to the user list
export const init_rt = (wss: WebSocket.Server) => {
    server = wss;

    wss.on("connection", (ws: WebSocket) => {
        console.log("Client connected, total clients: " + wss.clients.size);

        wsClientConnentedCallback();

        // Handle incoming messages from the client
        ws.on("message", (message: string) => {
            console.log(`Received message: ${message}`);

        });

        ws.on("close", () => { 
            console.log("Client disconnected, total clients: " + wss.clients.size);
        });
    });
}

export const sendToAll = (message: string) => {
    console.log("Sending message to all clients: " + message);

    server.clients.forEach((client: WebSocket) => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(message);
        }
    });
}