import express from "express";
//import compression from 'compression'
import { renderPage } from "vite-plugin-ssr";
import { createNextAuthMiddleware } from "./server/auth/auth";
import { createNextAuthOptions } from "./server/auth/auth.options";

/*import * as dotenv from 'dotenv' // see https://github.com/motdotla/dotenv#how-do-i-use-dotenv-with-import
dotenv.config();*/
const port = process.env.PORT;
const isProduction = process.env.NODE_ENV === "production";
const root = __dirname;

//console.log("node.env", process.env);
console.log("Production mode", process.env.NODE_ENV);
console.log("app root", __dirname);
console.log("app port", port);

startServer();

async function startServer() {
  const app = express();

  //app.use(compression())
  app.use(createNextAuthMiddleware(createNextAuthOptions()));

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
      session: res.locals.session,
      csrfToken: res.locals.csrfToken,
      callbackUrl: res.locals.callbackUrl
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
