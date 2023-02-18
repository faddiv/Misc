import express from "express";
//import compression from 'compression'
import { renderPage } from "vite-plugin-ssr";
import { root } from "./root.js";
const isProduction = process.env.NODE_ENV === "production";
if(!isProduction) {
  const dotenv = await import("dotenv");
  dotenv.config();
  /*dotenv.config({
    path: `${root}/.env`
  })*/
}
const port = process.env.PORT;

console.log("node.env", process.env);
console.log("Production mode", isProduction);
console.log("app root", root);
console.log("app port", port);

startServer();

async function startServer() {
  const app = express();

  //app.use(compression())

  if (isProduction) {
    //const sirv = (await import('sirv')).default
    //app.use(sirv(`${root}/dist/client`))
  } else {
    const vite = await import("vite");
    const viteServer = await vite.createServer({
      root,
      server: { middlewareMode: true },
      appType: "custom"
    });
    app.use(viteServer.middlewares);
  }

  app.get("*", async (req, res, next) => {
    const pageContextInit = {
      urlOriginal: req.originalUrl,
    };
    const pageContext = await renderPage(pageContextInit);
    const { httpResponse } = pageContext;
    if (!httpResponse) {
      return;
    }
    next();

    const { body, statusCode, contentType, earlyHints } = httpResponse;
    if (res.writeEarlyHints) {
      res.writeEarlyHints({ link: earlyHints.map((e) => e.earlyHintLink) });
    }
    res.status(statusCode).type(contentType).send(body);
  });

  app.listen(port || 3000);
  console.log(`Server running at http://localhost:${port || 3000}`);
}
