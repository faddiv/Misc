import { startServer } from "./server/server-main";

const root = __dirname;
console.log("app root", root);

startServer(root);
