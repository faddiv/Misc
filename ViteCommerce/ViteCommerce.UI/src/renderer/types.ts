import type { PageContextBuiltIn } from "vite-plugin-ssr";
// import type { PageContextBuiltInClient } from 'vite-plugin-ssr/client/router' // When using Client Routing
import type { PageContextBuiltInClient } from "vite-plugin-ssr/client"; // When using Server Routing
import { Session } from "next-auth";

type Page = (pageProps: PageProps) => React.ReactElement;
export type PageProps = {};

export type PageContextServerResult<T> = {
  pageContext: {
    pageProps: T;
  };
};

export interface PageContextInit {
  session: Session | null;
}
export interface PageContextInitServer extends PageContextInit {
  token?: string;
}

export interface PageContextClient2 extends PageContextInit {
  pageProps?: PageProps;
  urlPathname: string;
}

export interface PageContextServer2 extends PageContextClient2, PageContextInitServer {
  exports: {
    documentProps?: {
      title?: string;
      description?: string;
    };
  };
}

export type PageContextServer = PageContextBuiltIn<Page> & PageContextServer2;
export type PageContextClient = PageContextBuiltInClient<Page> & PageContextClient2;

export type PageContext = PageContextClient | PageContextServer;
