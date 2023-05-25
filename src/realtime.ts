import * as WebSocket from "ws";

let server : WebSocket.Server;

export const init_rt = (wss: WebSocket.Server) => {

    server = wss;

    console.log("init_rt called");
    wss.on("connection", (ws: WebSocket) => {
        console.log("Client connected, total clients: " + wss.clients.size);

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