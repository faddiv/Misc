import type { PageContextBuiltIn } from 'vite-plugin-ssr'
// import type { PageContextBuiltInClient } from 'vite-plugin-ssr/client/router' // When using Client Routing
import type { PageContextBuiltInClient } from "vite-plugin-ssr/client" // When using Server Routing
import { Session } from "next-auth";

type Page = (pageProps: PageProps) => React.ReactElement
export type PageProps = {}

export type PageContextServerResult<T> = {
  pageContext: {
    pageProps: T;
  }
}

export type PageContextCustom = {
  Page: Page,
  pageProps?: PageProps,
  urlPathname: string,
  session: Session | null,
  exports: {
    documentProps?: {
      title?: string
      description?: string
    }
  }
}

export type PageContextServer = PageContextBuiltIn<Page> & PageContextCustom & { baseUrl: string; token?: string };
export type PageContextClient = PageContextBuiltInClient<Page> & PageContextCustom;

export type PageContext = PageContextClient | PageContextServer;
