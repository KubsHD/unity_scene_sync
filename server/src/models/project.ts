import { Scene } from "./scene";
import { User } from "./user";

export class Project {
  id: string = "";
  users: User[] = [];
  scenes: Scene[] = [];
}
