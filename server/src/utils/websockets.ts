import { logger } from "@/server";
import { Server } from "http";
import * as WebSocket from "ws";
import { z } from "zod";
import { env } from "./env";

interface MessageHandlers {
  key: string;
  handler: (message: any) => void;
  schema: z.ZodSchema;
}

// array of message handlers to be registered with thier typesafe schema

let webSocketServer: WebSocket.Server;
let messageHandlers: MessageHandlers[] = [];
let users: Map<string, WebSocket.WebSocket> = new Map();

// payload structure
// {
//   type: "messageType",
//   payload: {
//     ...
//  }

// WebSockets only used to notify clients of changes to the user list
export const setupWebSockets = (httpServer: Server) => {
  webSocketServer = new WebSocket.Server({
    noServer: true,
    clientTracking: true,
    path: "/api/scene/ws",
  });

  logger.info("WebSockets server started");

  httpServer.on("upgrade", (request, socket, head) => {
    if (request.headers["key"] !== env.SECRET_KEY) {
      console.error("Invalid key: " + request.headers["key"]);
      socket.write("HTTP/1.1 401 Unauthorized\r\n\r\n");
      socket.destroy();
      return;
    }

    webSocketServer.handleUpgrade(request, socket, head, (ws) => {
      webSocketServer.emit("connection", ws, request);
    });
  });

  webSocketServer.on("connection", (ws: WebSocket) => {
    // @ts-ignore
    ws.uuid = crypto.randomUUID(); // Assign a unique ID to the WebSocket connection
    // @ts-ignore
    users.set(ws.uuid, ws); // Store the WebSocket connection in the users map

    logger.info("Client connected, total clients: " + users.size);

    // Handle incoming messages from the client
    ws.on("message", (message: string) => {
      //logger.info(`Received message: ${message}`);

      let parsedMessage: any;
      try {
        parsedMessage = JSON.parse(message);
      } catch (e) {
        console.error("Failed to parse message: " + message);
        return;
      }

      const messageType = parsedMessage.type;
      const messagePayload = parsedMessage.payload;

      const handler = messageHandlers.find((h) => h.key === messageType);
      if (handler) {
        try {
          handler.handler(messagePayload);
        } catch (error) {
          logger.error(`Error handling message type ${messageType}:`, error);
        }
      } else {
        //logger.warn(`Unknown message type: ${messageType}`);
      }
    });

    ws.on("close", () => {
      // @ts-ignore
      users.delete(ws.uuid); // Remove the WebSocket connection from the users map

      logger.info(
        `Client disconnected, total clients: ${webSocketServer.clients.size}`
      );
    });
  });
};

export const sendToAll = (message: any) => {
  logger.info("Sending message to all clients: " + message);

  webSocketServer.clients.forEach((client: WebSocket.WebSocket) => {
    if (client.readyState === WebSocket.OPEN) {
      client.send(message);
    }
  });
};

export const registerMessageHandler = <T>(
  messageType: string,
  handler: (message: T) => void
) => {
  messageHandlers.push({
    key: messageType,
    handler: handler,
    schema: z.object({ type: z.literal(messageType) }),
  });
};
