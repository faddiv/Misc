import type { Express, Response } from "express-serve-static-core";
import { renderPage } from "vite-plugin-ssr";
import { ResLocals } from "../auth/locals";

export function addViteSsr(app: Express) {
  app.get("*", async (req, res: Response<any, ResLocals>, next) => {
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
}
