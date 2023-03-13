import type { Express, Response } from "express-serve-static-core";
import { createProxyMiddleware } from "http-proxy-middleware";
import { ResLocals } from "../auth/locals";

const apiUrl = process.env.API_URL;
console.log("api url", apiUrl);

export function addProxy(app: Express) {
  const apiProxy = createProxyMiddleware({
    target: apiUrl, // target host with the same base path
    changeOrigin: true, // needed for virtual hosted sites
    secure: false,
    onProxyReq(proxyReq, _req, res: Response<any, ResLocals>, _options) {
      const token = res.locals.token;
      if (token) {
        //console.log("token provided", token);
        proxyReq.setHeader("Authorization", `Bearer ${token.id_token}`);
      }
    },
  });
  app.use("/api", apiProxy);
}
