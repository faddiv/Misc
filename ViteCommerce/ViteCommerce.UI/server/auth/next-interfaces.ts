import type NextAuth from "next-auth";
import type { NextAuthOptions } from "next-auth";
import type { Request, Response, NextFunction } from 'express'

/**
 * Next `API` route request
 */
declare type NextApiRequest = Parameters<typeof NextAuth>[0];

/**
 * Next `API` route response
 */
declare type NextApiResponse = Parameters<typeof NextAuth>[1];

export type { NextApiRequest, NextApiResponse, NextRequestHandler };

type NextRequestHandler = (
  req: NextApiRequest & Request,
  res: NextApiResponse & Response,
  next: NextFunction,
  opts: NextAuthOptions
) => any;
