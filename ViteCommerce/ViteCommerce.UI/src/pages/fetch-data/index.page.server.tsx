import type {
  PageContextServer,
  PageContextServerResult,
} from "../../renderer/types";
import { FetchDataProps } from "./FetchDataProps";
import { fetchWeatherforecast } from "./WeatherApiClient";

export async function onBeforeRender(
  pageContext: PageContextServer
): Promise<PageContextServerResult<FetchDataProps>> {
  try {
    if (!pageContext.session) {
      return {
        pageContext: {
          pageProps: {
            error: "Please login",
          },
        },
      };
    }
    const data = await fetchWeatherforecast(pageContext.session);
    return {
      pageContext: {
        pageProps: {
          data,
        },
      },
    };
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
