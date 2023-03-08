import type { Session } from "next-auth";

export async function fetchWeatherforecast(session: Session) {
  const response = await fetch("https://localhost:5001/weatherforecast", {
    headers: {
      Authorization: `Bearer ${session.id_token}`,
    },
  });
  if (response.ok) {
    return await response.json();
  } else {
    throw new Error(response.statusText);
  }
}
