import type { PageContextServer, PageContextServerResult } from "../../renderer/types";
import { FetchDataProps } from "./FetchDataProps";
import { fetchWeatherforecast } from "./WeatherApiClient";

export async function onBeforeRender(pageContext: PageContextServer): Promise<PageContextServerResult<FetchDataProps>> {
  try {
    const data = await fetchWeatherforecast(pageContext);
    return {
      pageContext: {
        pageProps: {
          data,
        },
      },
    };
  } catch (error) {
    return {
      pageContext: {
        pageProps: {
          error: (error as Error).message,
        },
      },
    };
  }
}
