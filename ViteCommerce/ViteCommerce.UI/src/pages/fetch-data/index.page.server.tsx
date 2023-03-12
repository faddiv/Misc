import type {
  PageContextServer,
  PageContextServerResult,
} from "../../renderer/types";
import { FetchDataProps } from "./FetchDataProps";
import { fetchWeatherforecastServer } from "./WeatherApiClient";

export async function onBeforeRender(
  pageContext: PageContextServer
): Promise<PageContextServerResult<FetchDataProps>> {
  const data = await fetchWeatherforecastServer(pageContext);
  return {
    pageContext: {
      pageProps: {
        data,
      },
    },
  };
}
