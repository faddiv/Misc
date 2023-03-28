import type { JWT } from "next-auth/jwt";
import type { Session } from "next-auth";

declare module "next-auth" {
  interface Session {
    session_id?: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    session_id?: string;
  }
}
