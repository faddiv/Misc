import type { JWT } from "next-auth/jwt";
import type { Session } from "next-auth";

declare module "next-auth" {
  interface Session {
    // Add session data here
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    // add jwt data here
  }
}
