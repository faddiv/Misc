import NextAuth, { getServerSession } from "next-auth/next";
import type { AuthOptions } from "next-auth";
import type { NextApiRequest, NextApiResponse } from "./next-interfaces";
import { Router } from "express";
import type { RequestHandler } from "express";
import  { json, urlencoded } from "body-parser";
import cookieParser from "cookie-parser";
import type { ResLocals } from "./locals";
import { getToken } from "next-auth/jwt";

function createRequestHandler(options: AuthOptions): RequestHandler<any, any, any, { nextauth: undefined | string[] }> {
  return (req, res, _) => {
    const nextauth = req.path.split("/");
    nextauth.splice(0, 3);
    req.query.nextauth = nextauth;
    const nextRequest = Object.assign(req, {
      env: process.env,
    });
    const nextResponse = res as unknown as NextApiResponse;
    console.log("GET nextauth", nextauth.join("; "));
    NextAuth(nextRequest, nextResponse, options);
  };
}

function createSessionHandler(options: AuthOptions): RequestHandler<any, any, any, any, ResLocals> {
  return async (req, res, next) => {
    const session = await getServerSession(req as unknown as NextApiRequest, res as unknown as NextApiResponse, options);
    res.locals.session = session || undefined;
    res.locals.token = await getToken({
      req,
      raw:true
    });
    //console.log("Got raw token", res.locals.token);
    next();
  };
}

export function createNextAuthMiddleware(options: AuthOptions) {
  return Router()
    .use(urlencoded({ extended: false }))
    .use(json())
    .use(cookieParser())
    .get("/api/auth/*", createRequestHandler(options))
    .post("/api/auth/*", createRequestHandler(options))
    .all("*", createSessionHandler(options));
}
