import { PageContext, PageContextServer } from "../../renderer/types";
import { fetch } from "./serverFetch";

export async function fetchWeatherforecast(pageContext: PageContext) {
  console.log("Calling fetch");
  const response = await fetch("/api/weatherforecast");
  if (response.ok) {
    return await response.json();
  } else {
    throw new Error(response.statusText);
  }
}

export async function fetchWeatherforecastServer(pageContext: PageContextServer) {
  const response = await fetch("/api/weatherforecast", {
    headers: {
      Authorization: `Bearer ${pageContext.token}`,
    },
  });
  if (response.ok) {
    return await response.json();
  } else {
    throw new Error(response.statusText);
  }
}
