import { PageContext, PageContextServer } from "../../renderer/types";
import { fetch } from "./serverFetch";

export async function fetchWeatherforecast(pageContext: PageContext | PageContextServer) {
  const headers = new Headers();
  if (hasToken(pageContext)) {
    headers.append("Authorization", `Bearer ${pageContext.token}`);
    //console.log("Server call with token");
  }
  const response = await fetch("/api/weatherforecast", {
    headers: headers,
  });
  if (response.ok) {
    return await response.json();
  } else {
    throw new Error(response.statusText);
  }
}
function hasToken(pageContext: PageContext | PageContextServer): pageContext is PageContextServer {
  return import.meta.env.SSR && !!(pageContext as PageContextServer).token;
}
export async function fetchWeatherforecastServer(pageContext: PageContextServer) {

}
