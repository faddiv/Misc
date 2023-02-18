import { PropsWithChildren } from "react";
import { Container, SSRProvider } from "react-bootstrap";
import { PageContext } from "../../renderer/types";
import { PageContextProvider } from "../../renderer/usePageContext";
import { Menu } from "./Menu";

export function Layout({
  children,
  pageContext,
}: PropsWithChildren & { pageContext: PageContext }) {
  return (
    <PageContextProvider pageContext={pageContext}>
      <SSRProvider>
        <Menu />
        <Container className="py-4">{children}</Container>
      </SSRProvider>
    </PageContextProvider>
  );
}
