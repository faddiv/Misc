import type { Session } from "next-auth";

export interface ResLocals {
  token?: string;
  session?: Session;
}
