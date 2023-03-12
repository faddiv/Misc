import express from "express";
import { createProxyMiddleware } from "http-proxy-middleware";
import { getToken } from "next-auth/jwt";
//import compression from 'compression'
import { renderPage } from "vite-plugin-ssr";
import { createNextAuthMiddleware } from "./server/auth/auth";
import { createNextAuthOptions } from "./server/auth/auth.options";

/*import * as dotenv from 'dotenv' // see https://github.com/motdotla/dotenv#how-do-i-use-dotenv-with-import
dotenv.config();*/
const port = process.env.PORT;
const isProduction = process.env.NODE_ENV === "production";
const root = __dirname;
const apiUrl = process.env.API_URL;

//console.log("node.env", process.env);
console.log("Production mode", process.env.NODE_ENV);
console.log("app root", __dirname);
console.log("app port", port);
console.log("api url", apiUrl);

startServer();

async function startServer() {
  const app = express();

  //app.use(compression())
  app.use(createNextAuthMiddleware(createNextAuthOptions()));

  const apiProxy = createProxyMiddleware({
    target: apiUrl, // target host with the same base path
    changeOrigin: true, // needed for virtual hosted sites
    secure: false,
    onProxyReq(proxyReq, _req, res, _options) {
      const token = res.locals.token;
      if (token) {
        //console.log("token provided", token);
        proxyReq.setHeader("Authorization", `Bearer ${token.id_token}`);
      }
    },
  });
  app.use("/api", apiProxy);

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

  app.get("*", async (req, res, next) => {
    const pageContextInit = {
      urlOriginal: req.originalUrl,
      session: res.locals.session,
      token: res.locals.token?.id_token,
      csrfToken: res.locals.csrfToken,
      callbackUrl: res.locals.callbackUrl,
      baseUrl: req.baseUrl,
    };
    const pageContext = await renderPage(pageContextInit);

    // An error occured during server-side rendering
    if (pageContext.errorWhileRendering) {
      console.log("Error while rendering:", pageContext.errorWhileRendering);
    }

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
