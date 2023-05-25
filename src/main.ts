import express, { Application, Request, Response } from 'express';
import * as WebSocket from "ws";
import * as http from "http";
import { init_rt, sendToAll } from './realtime';
import basicAuth from "express-basic-auth";

const app = express();
const server = http.createServer(app);

const wss: WebSocket.Server = new WebSocket.Server({ server, path: "/rt" });

init_rt(wss);

const PORT: number = 3060;

class User {
  id: string;
  name: string;
  scene: string;
  time: number;

  constructor(json: any) {
    this.name = json.name;
    this.id = json.id;
    this.scene = json.scene;
    this.time = Date.now();
  }

  update(newUserInfo: User): void {
    this.scene = newUserInfo.scene;
    this.time = Date.now();
  };
}

const users: User[] = [];

app.use(express.json());
app.use(basicAuth({
    users: { 'strb': 'xww^2L8a$BH6UCEcZ!H$3Bc7#Npoc%xf53BtKC!phJi7QWaWVhj4T^K' }
}))

app.post('/logoutUser', (req: Request, res: Response): void => {

  console.log(req.body.name + " wants to log out")

  let u: User = users.find((u: User) => u.id == req.body.id);
  if (u) {
    var index = users.indexOf(u);
    if (index !== -1) {
      users.splice(index, 1);
      res.send("Removed user");
    }
    sendToAll(JSON.stringify(users));
  }
  else {
    res.sendStatus(400);
  }

});

app.post('/sendUserInfo', (req: Request, res: Response): void => {

  if (req.body.id === undefined) {
    res.sendStatus(400);
    return;
  }

  let user: User = new User(req.body);

  // find user in array called users with id of user.id
  let u: User = users.find((u: User) => u.id == user.id);

  if (u) {

    // update entry
    u.update(user);

    console.log(user.name + " updated")

    res.send("User Updated")
  }
  else {
    // create entry
    users.push(user);

    console.log(user.name + " registered")

    res.send("User Registered")
  }

  sendToAll(JSON.stringify(users));

});

app.post("/getPeopleOnScene", (req: Request, res: Response) => {
  console.log(req.body.scene)
  console.log(users)

  let usersOnScene: User[] = structuredClone(users.filter((u: User) => u.scene == req.body.scene));

  // change id for all users 
  usersOnScene.forEach((u: User) => u.id = "READACTED");

  res.send(usersOnScene);
});

server.listen(3060, () => {
  console.log("Server started on port 3060");
});

