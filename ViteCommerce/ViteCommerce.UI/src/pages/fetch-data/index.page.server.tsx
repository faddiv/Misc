import css from "./index.module.scss";
import reactLogo from "../../assets/logo.svg";
import { useState } from "react";
import { Button } from "react-bootstrap";
import {
  PageContextServer,
  PageContextServerResult,
} from "../../renderer/types";
import { FetchDataProps } from "./FetchDataProps";

export async function onBeforeRender(
  pageContext: PageContextServer
): Promise<PageContextServerResult<FetchDataProps>> {
  try {
    const response = await fetch("https://localhost:5001/weatherforecast");
    if (response.ok) {
      const data = await response.json();
      return {
        pageContext: {
          pageProps: {
            data,
          },
        },
      };
    } else {
      return {
        pageContext: {
          pageProps: {
            error: response.statusText,
          },
        },
      };
    }
  } catch (error) {
    console.log("Server fetch failed", error);
    return {
      pageContext: {
        pageProps: {
          error: "server fetch failed",
        },
      },
    };
  }
}
