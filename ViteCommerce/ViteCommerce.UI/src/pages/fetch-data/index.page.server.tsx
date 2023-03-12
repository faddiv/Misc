import type {
  PageContextServer,
  PageContextServerResult,
} from "../../renderer/types";
import { FetchDataProps } from "./FetchDataProps";
import { fetchWeatherforecastServer } from "./WeatherApiClient";

export async function onBeforeRender(
  pageContext: PageContextServer
): Promise<PageContextServerResult<FetchDataProps>> {

  try {
    const data = await fetchWeatherforecastServer(pageContext);
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
