import css from "./index.module.scss";
import reactLogo from "../../assets/logo.svg";
import { useState } from "react";
import { Button } from "react-bootstrap";
import { PageContextServer } from "../../renderer/types";

export async function onBeforeRender(pageContext: PageContextServer) {
  console.log("Server fetch called");
  /*const pageProps = { data: "Hello" };
    return {
      pageContext: {
        pageProps,
      },
    };*/
  try {
    const response = await fetch("http://vitecommerce-api/weatherforecast");
  if(response.ok) {
    const data = await response.json();
    const pageProps = { data };
    return {
      pageContext: {
        pageProps,
      },
    };
  } else {
    const pageProps = { error: response.statusText };
    return {
      pageContext: {
        pageProps,
      },
    };
  }
  } catch (error) {
    console.log("Server fetch failed", error);
    const pageProps = { error: "server fetch failed" };
    return {
      pageContext: {
        pageProps,
      },
    };
  }

}
