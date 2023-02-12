import css from "./index.module.scss";
import reactLogo from "../../assets/logo.svg";
import { useState } from "react";
import { Button } from "react-bootstrap";
import { PageContextServer } from "../../renderer/types";

export async function onBeforeRender(pageContext: PageContextServer) {
  console.log("Server fetch called");
  const data = await new Promise<string>((accept) => {
    setTimeout(() => {
      accept("Server fetched data");
    }, 300);
  });

  const pageProps = { data };
  return {
    pageContext: {
      pageProps,
    },
  };
}
