import Google from "next-auth/providers/google";
import Duende from "next-auth/providers/duende-identity-server6";
import type { AuthOptions } from "next-auth";
import { SignJWT, decodeJwt } from "jose";
import hkdf from "@panva/hkdf";

async function getDerivedEncryptionKey(secret: string) {
  return await hkdf("sha256", secret, "", "NextAuth.js Generated Encryption Key", 32);
}
const now = () => (Date.now() / 1000) | 0;
const DEFAULT_MAX_AGE = 30 * 24 * 60 * 60;

export function createNextAuthOptions() {
  if (!process.env.NEXTAUTH_SECRET) {
    throw new Error("Please define NEXTAUTH_SECRET in the .env file.");
  }

  const nextAuthOptions: AuthOptions = {
    //debug: true,
    secret: process.env.NEXTAUTH_SECRET,
    session: {
      strategy: "jwt",
      maxAge: 24 * 60 * 60,
    },
    providers: [],
    jwt: {
      async encode({ secret, maxAge, token }) {
        if (token) {
          // jwe with dir is not supported by asp.net. Create jwt instead.
          const encryptionSecret = await getDerivedEncryptionKey(secret as string);
          console.log("token:", token);
          const enc = await new SignJWT(token)
            .setProtectedHeader({
              alg: "HS256",
              enc: "dir"
            })
            .setIssuedAt()
            .setExpirationTime(now() + (maxAge || DEFAULT_MAX_AGE))
            .setAudience(process.env.NEXTAUTH_AUDIENCE || "localhost")
            .setIssuer("http://localhost:3000")
            //.setJti("jtiValue")
            .sign(encryptionSecret);
          //console.log("encoded token:", enc);
          return enc;
        } else {
          return "";
        }
      },
      async decode(params) {
        const { token } = params;
        if (!token) return null;
        const payload = decodeJwt(token);
        //console.log("decoded payload:", payload);
        return payload;
      },
    },
    callbacks: {
      async jwt({ token, account }) {
        if (account) {
          //console.log("token:", token, "account:", account);
          // add data here into the jwt.
          switch (account.provider) {
            case "duende-identityserver6":
              // get data from here
              break;
            case "google":
              // get data from here
              break;
            default:
              break;
          }
        }
        return token;
      },
      session({ session, token, user }) {
        // add data to session here. This usable on client side.
        //console.log("session:", session, "token:", token);
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
    console.log("Google provider added.");
  }

  if (process.env.DUENDE_CLINET_ID && process.env.DUENDE_ISSUER) {
    nextAuthOptions.providers.push(
      Duende({
        clientId: process.env.DUENDE_CLINET_ID,
        clientSecret: process.env.DUENDE_CLINET_SECRET || "",
        issuer: process.env.DUENDE_ISSUER,
        authorization: {
          params: {
            scope: "openid profile",
          },
        },
      })
    );
    if (process.env.DUENDE_CLINET_SECRET) {
      console.log("Duende provider added with secret.");
    } else {
      console.log("Duende provider added without secret.");
    }
  }
  return nextAuthOptions;
}
