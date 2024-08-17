import { Project } from "@/models/project";

let projects: Project[] = [];

export const Database = {
  projects,
  getProjectById: (id: string) => {
    return projects.find((p) => p.id == id);
  },
};
