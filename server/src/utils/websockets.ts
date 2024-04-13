import { logger } from "@/server";
import { Server } from "http";
import * as WebSocket from "ws";
import { z } from "zod";

interface MessageHandlers {
  key: string;
  handler: (message: any) => void;
  schema: z.ZodSchema;
}

// array of message handlers to be registered with thier typesafe schema

let server: WebSocket.Server;
let messageHandlers: MessageHandlers[];

// payload structure
// {
//   type: "messageType",
//   payload: {
//     ...
//  }

// WebSockets only used to notify clients of changes to the user list
export const setupWebSockets = (wss: Server) => {
  server = new WebSocket.Server({
    server: wss,
    path: "/api/scene/ws",
  });

  logger.info("WebSockets server started");

  wss.on("connection", (ws: WebSocket) => {
    console.log(
      "Client connected, total clients: " + (server.clients.size + 1)
    );

    // Handle incoming messages from the client
    ws.on("message", (message: string) => {
      console.log(`Received message: ${message}`);

      let parsedMessage: any;
      try {
        parsedMessage = JSON.parse(message);
      } catch (e) {
        console.error("Failed to parse message: " + message);
        return;
      }

      const messageType = parsedMessage.type;
      const messagePayload = parsedMessage.payload;
      messageHandlers
        .find((handler) => handler.key === messageType)
        ?.handler(messagePayload);
    });

    ws.on("close", () => {
      console.log(
        "Client disconnected, total clients: " + (server.clients.size - 1)
      );
    });
  });
};

export const sendToAll = (message: any) => {
  logger.info("Sending message to all clients: " + message);

  server.clients.forEach((client: WebSocket.WebSocket) => {
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
