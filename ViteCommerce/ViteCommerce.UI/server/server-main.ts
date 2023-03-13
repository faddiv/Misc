import express from "express";
//import compression from 'compression'
import { createNextAuthMiddleware } from "./auth/auth";
import { createNextAuthOptions } from "./auth/auth.options";
import { addProxy } from "./proxy";
import { addViteSsr } from "./vite-ssr";

const port = process.env.PORT;
const isProduction = process.env.NODE_ENV === "production";
//console.log("node.env", process.env);
console.log("Production mode", process.env.NODE_ENV);
console.log("app port", port);

export async function startServer(root: string) {
  const app = express();

  //app.use(compression())
  app.use(createNextAuthMiddleware(createNextAuthOptions()));
  addProxy(app);

  if (isProduction) {
    //const sirv = (await import('sirv')).default
    //app.use(sirv(`${root}/dist/client`))
  } else {
    const vite = await import("vite");
    const viteServer = await vite.createServer({
      root,
      server: { middlewareMode: true },
      appType: "custom",
    });
    app.use(viteServer.middlewares);
  }

  addViteSsr(app);

  app.listen(port || 3000);
  console.log(`Server running at http://localhost:${port || 3000}`);
}
