import Google from "next-auth/providers/google";
import Duende from "next-auth/providers/duende-identity-server6";
import type { AuthOptions } from "next-auth";
//const Google = GoogleModule.default;
//const Duende = DuendeModule.default;

export function createNextAuthOptions() {
  if (!process.env.NEXTAUTH_SECRET) {
    throw new Error("Please define NEXTAUTH_SECRET in the .env file.");
  }

  const nextAuthOptions: AuthOptions = {
    debug: true,
    secret: process.env.NEXTAUTH_SECRET,
    session: {
      strategy: "jwt",
      maxAge: 24 * 60 * 60,
    },
    providers: [],
    callbacks: {
      jwt({ token, account, profile }) {
        //console.log("jwt callback:", token, account, profile);
        if (account) {
          token.id_token = account.id_token;
        }
        return token;
      },
      session({ session, token, user }) {
        //console.log("session callback:", session, token, user);
        //session.sub = token.sub;
        //session.id_token = token.id_token as string | undefined;
        //session.access_token = token.access_token as string | undefined;
        return session;
      },
    },
  };

  if (process.env.GOOGLE_CLINET_ID && process.env.GOOGLE_CLINET_SECRET) {
    nextAuthOptions.providers.push(
      Google({
        clientId: process.env.GOOGLE_CLINET_ID,
        clientSecret: process.env.GOOGLE_CLINET_SECRET,
      })
    );
  }

  if (process.env.DUENDE_CLINET_ID && process.env.DUENDE_ISSUER) {
    nextAuthOptions.providers.push(
      Duende({
        clientId: process.env.DUENDE_CLINET_ID,
        clientSecret: process.env.DUENDE_CLINET_SECRET || "",
        issuer: process.env.DUENDE_ISSUER,
        authorization: {
          params: {
            scope: "ViteCommerce.ApiAPI openid profile",
          },
        },
      })
    );
  }
  return nextAuthOptions;
}
