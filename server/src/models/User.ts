export class User {
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
  }
}
