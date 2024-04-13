import express, { Application, Request, Response, Router } from "express";
import { User } from "@/models/user";
import { logger } from "@/server";
import { sceneInfoService } from "@/service/sceneInfoService";
import { Database } from "@/db/database";
import { sendToAll } from "@/utils/websockets";

export const sceneInfoRouter: Router = (() => {
  const router: Router = Router({ mergeParams: true });

  router.post("/loginUser", async (req: Request, res: Response) => {
    logger.info(req.params.project + ": " + req.body.name + " wants to log in");

    const users = await sceneInfoService.updateUserOnScene(
      req.params.project,
      req.body
    );

    logger.debug(users);

    sendToAll(JSON.stringify(users));

    res.sendStatus(200);
  });

  router.post("/logoutUser", async (req: Request, res: Response) => {
    logger.info(
      req.params.project + ": " + req.body.name + " wants to log out"
    );

    const users = await sceneInfoService.logoutUser(
      req.params.project,
      req.body.id
    );

    sendToAll(JSON.stringify(users));

    res.sendStatus(200);
  });

  router.post("/updateUser", async (req: Request, res: Response) => {
    if (req.body.id === undefined) {
      res.sendStatus(400);
      return;
    }

    const users = await sceneInfoService.updateUserOnScene(
      req.params.project,
      req.body
    );

    sendToAll(JSON.stringify(users));
  });

  router.post("/getPeopleOnScene", async (req: Request, res: Response) => {
    res.send(
      await sceneInfoService.getSceneInfo(req.params.project, req.body.scene)
    );
  });

  return router;
})();
