import { app, logger } from "@/server";
import { env } from "@/utils/env";
import { registerMessageHandler, setupWebSockets } from "@/utils/websockets";
import { Server } from "http";
import { sceneInfoService } from "./service/sceneInfoService";

if (env.SECRET_KEY === undefined) {
  logger.error("SECRET_KEY not set");
  process.exit(1);
}

const server: Server = app.listen(env.PORT, () => {
  const { NODE_ENV, HOST, PORT } = env;
  logger.info(`Server (${NODE_ENV}) running on port http://${HOST}:${PORT}`);
  setupWebSockets(server);
});

const onCloseSignal = () => {
  logger.info("sigint received, shutting down");
  server.close(() => {
    logger.info("server closed");
    process.exit();
  });
  setTimeout(() => process.exit(1), 10000).unref(); // Force shutdown after 10s
};

process.on("SIGINT", onCloseSignal);
process.on("SIGTERM", onCloseSignal);
