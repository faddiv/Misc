import css from "./index.module.scss";
import reactLogo from "../../assets/logo.svg";
import { useState } from "react";
import { Button } from "react-bootstrap";
import { PageContextClient } from "../../renderer/types";

export function Page({ data }: any) {
  const [count, setCount] = useState(0);
  return (
    <div className={css.app}>
      <div>
        <a href="https://vitejs.dev" target="_blank">
          <img src="/vite.svg" className="logo" alt="Vite logo" />
        </a>
        <a href="https://reactjs.org" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Vite + React</h1>
      <div className={css.card}>
        <Button
          variant="primary"
          onClick={() => setCount((count) => count + 1)}
        >
          count is {count}
        </Button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <p className={css.readTheDocs}>
        Click on the Vite and React logos to learn more
      </p>
      <p>Data: {data}</p>
    </div>
  );
}

export async function onBeforeRender(pageContext: PageContextClient) {
  console.log("Clinet fetch called");
  const data = await new Promise<string>((accept) => {
    setTimeout(() => {
      accept("Client fetched data");
    }, 300);
  });

  const pageProps = { data };
  return {
    pageContext: {
      pageProps,
    },
  };
}
