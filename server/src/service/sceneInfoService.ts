import { Database } from "@/db/database";
import { User } from "@/models/user";
import { logger } from "@/server";
import { registerMessageHandler } from "@/utils/websockets";

const getProject = (project: string) => {
  const foundProject = Database.projects.find((p) => p.id == project);
  if (foundProject) return foundProject;
  else {
    const newProject = { id: project, users: [], scenes: [] };
    Database.projects.push(newProject);
    return newProject;
  }
};

export const sceneInfoService = {
  addUser: async (projectId: string, userData: any) => {
    const project = getProject(projectId);

    if (!project) {
      return null;
    }
    const user = new User(userData);

    project.users.push(user);

    logger.info(`[${project.id}] ${user.name} is now in scene ${user.scene}`);

    return structuredClone(project.users);
  },
  getSceneInfo: async (projectId: string, sceneName: string) => {
    const project = getProject(projectId);

    if (!project) {
      return null;
    }

    const users = structuredClone(
      project.users.filter((u: User) => u.scene == sceneName)
    );
    users.forEach((u: User) => (u.id = ""));
    return users;
  },
  updateUserOnScene: async (projectId: string, userData: any) => {
    const project = getProject(projectId);

    if (!project) {
      return null;
    }

    const user = new User(userData);

    let u: User | undefined = project.users.find((u: User) => u.id == user.id);

    if (u) {
      u.update(user);
    } else {
      project.users.push(user);
    }

    logger.info(`[${project.id}] ${user.name} is now in scene ${user.scene}`);

    return structuredClone(project.users);
  },
  logoutUser: async (projectId: string, userId: string) => {
    const project = getProject(projectId);

    if (!project) {
      return null;
    }

    let u: User | undefined = project.users.find((u: User) => u.id === userId);

    if (u) {
      console.log("Logging out user:", u);
      var index = project.users.indexOf(u);
      if (index !== -1) {
        project.users.splice(index, 1);
      }
    }

    return structuredClone(project.users);
  },
  handleHeartbeat: async (message: any) => {},
};
