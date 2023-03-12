import NextAuth, { getServerSession } from "next-auth/next";
import type { AuthOptions, Session } from "next-auth";
import type { NextApiRequest, NextApiResponse } from "./next-interfaces";
import { Router } from "express";
import type { RequestHandler } from "express";
import bodyParser from "body-parser";
import cookieParser from "cookie-parser";
import { getToken } from "next-auth/jwt";
const { json, urlencoded } = bodyParser;

function createRequestHandler(options: AuthOptions): RequestHandler<any,any, any, {nextauth: undefined | string[]}> {
  return (req, res, _) => {
    const nextauth = req.path.split("/");
    nextauth.splice(0, 3);
    req.query.nextauth = nextauth;
    const nextRequest = Object.assign(req, {
      env: process.env
    });
    const nextResponse = res as unknown as NextApiResponse;
    console.log("GET nextauth", nextauth.join('; '));
    NextAuth(nextRequest, nextResponse, options);
  };
}

function createSessionHandler(options: AuthOptions): RequestHandler<any,any, any, {nextauth: undefined | string[]}>  {
  return async (req, res, next) => {
    const session: Session | null = await getServerSession(req as unknown as NextApiRequest, res as unknown as NextApiResponse, options);
    const token = await getToken({ req });
    res.locals.session = session;
    res.locals.token = token;
    next();
  }
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
