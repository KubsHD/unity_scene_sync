import express, { Application, Request, Response } from "express";
const basicAuth = require("express-basic-auth");
import pino from "pino";

import { sceneInfoRouter } from "@/routes/sceneInfoRouter";
import { env } from "@/utils/env";
import errorMiddleware from "./utils/errorMiddleware";

const app = express();
const logger = pino({ name: "server start" });

app.use(express.json());
//app.use(errorMiddleware());

if (env.NODE_ENV !== "development") {
  app.use(
    basicAuth({
      users: { "": process.env.SECRET_KEY },
    })
  );
}

app.use("/api/scene/:project", sceneInfoRouter);

export { app, logger };
