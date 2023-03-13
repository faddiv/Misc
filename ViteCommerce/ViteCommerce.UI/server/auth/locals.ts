import type { Session } from "next-auth";
import type { JWT } from "next-auth/jwt";

export interface ResLocals {
  token?: JWT | null;
  session?: Session;
  csrfToken?: string;
  callbackUrl?: string;
}
