import { useCallback, useState } from "react";
import { Button, Stack } from "react-bootstrap";
import { FetchDataProps } from "./FetchDataProps";
import { fetchWeatherforecast } from "./WeatherApiClient";
import { WeatherForecast } from "../../components/weatherForecast";
import { usePageContext } from "../../renderer/usePageContext";

export function Page({ data, error }: FetchDataProps) {
  const [forecasts, setForecasts] = useState<{
    data: WeatherForecast[] | undefined;
    loading: boolean;
    error: string | undefined;
  }>({ data, loading: false, error });
  const pageContext = usePageContext();
  const refresh = useCallback(async () => {
    //if (!session.data) return;
    setForecasts((prev) => ({
      data: prev.data,
      loading: true,
      error: undefined,
    }));
    try {
      const data = await fetchWeatherforecast(pageContext);
      setForecasts({
        data,
        loading: false,
        error: undefined,
      });
    } catch (error) {
      setForecasts({
        data: undefined,
        loading: false,
        error: (error as Error).message,
      });
    }
  }, []);
  return (
    <Stack>
      <Stack direction="horizontal" className="ms-auto">
        <Button onClick={refresh} disabled={forecasts.loading}>
          {forecasts.loading ? "Loading" : "Refresh"}
        </Button>
      </Stack>
      {!!forecasts.error ? (
        <div>
          <h1>Error occured</h1>
          <h2>{forecasts.error}</h2>
        </div>
      ) : (
        <table className="table table-striped" aria-labelledby="tableLabel">
          <thead>
            <tr>
              <th>Date</th>
              <th>Temp. (C)</th>
              <th>Temp. (F)</th>
              <th>Summary</th>
            </tr>
          </thead>
          <tbody>
            {forecasts.data?.map((forecast) => (
              <tr key={forecast.date}>
                <td>{forecast.date}</td>
                <td>{forecast.temperatureC}</td>
                <td>{forecast.temperatureF}</td>
                <td>{forecast.summary}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </Stack>
  );
}
