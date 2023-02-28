import { Root, createRoot, hydrateRoot } from "react-dom/client";
import "./index.scss";
import { PageContextClient } from "./types";
import { Layout } from "../components/layoutElements/Layout";



export async function render(pageContext: PageContextClient) {
  const { Page, pageProps, isHydration } = pageContext;
  renderRoot(
    isHydration,
    <Layout pageContext={pageContext}>
      <Page {...pageProps} />
    </Layout>
  );
}

let root: Root | null = null;
function renderRoot(isHydration: boolean, page: React.ReactNode) {
  if (!root) {
    const container = document.getElementById("root") as HTMLElement;
    if (isHydration) {
      root = hydrateRoot(container, page);
    } else {
      if (!root) {
        root = createRoot(container);
      }
    }
  }
  root.render(page);
}

export const clientRouting = true;
