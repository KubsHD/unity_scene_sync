import { Database } from "@/db/database";
import { User } from "@/models/user";
import { storbeed } from "@/protos";
import { logger } from "@/server";
import { registerMessageHandler, sendToAll } from "@/utils/websockets";

export const sceneLockService = {
  lockScene: (
    projectId: string,
    sceneName: string,
    userId: string
  ): boolean => {
    const project = Database.projects.find((p) => p.id == projectId);

    if (!project) {
      return false;
    }

    const scene = project.scenes.find((s) => s.name == sceneName);

    if (!scene) {
      project.scenes.push({
        name: sceneName,
        lockedBy: userId,
        isLocked: true,
      });
    } else {
      if (scene.isLocked) {
        logger.info(
          `[${project.id}] ${userId} tried to lock scene ${sceneName} but failed since it's locked by ${scene.lockedBy}`
        );
        return false;
      }

      scene.lockedBy = userId;
      scene.isLocked = true;
    }

    logger.info(`[${project.id}] ${userId} locked scene ${sceneName}`);
    return true;
  },
  unlockScene: (
    projectId: string,
    sceneName: string,
    userId: string
  ): boolean => {
    const project = Database.projects.find((p) => p.id == projectId);

    if (!project) {
      return false;
    }

    const scene = project.scenes.find((s) => s.name == sceneName);

    if (!scene) {
      return false;
    }

    if (scene.lockedBy !== userId) {
      logger.info(
        `[${project.id}] ${userId} tried to unlock scene ${sceneName} but failed since it's locked by ${scene.lockedBy}`
      );
      return false;
    }

    scene.isLocked = false;
    scene.lockedBy = "";
    logger.info(`[${project.id}] ${userId} unlocked scene ${sceneName}`);

    return true;
  },
};
