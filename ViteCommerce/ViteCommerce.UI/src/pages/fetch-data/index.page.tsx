import { useCallback, useState } from "react";
import { Button, Col, Row, Stack } from "react-bootstrap";
import { FetchDataProps } from "./FetchDataProps";

export function Page({ data, error }: FetchDataProps) {
  const [forecasts, setForecasts] = useState({ data, loading: false });
  const refresh = useCallback(async () => {
    setForecasts((prev) => ({
      data: prev.data,
      loading: true,
    }));
    const response = await fetch("https://localhost:5001/weatherforecast");
    if (response.ok) {
      const data = await response.json();
      setForecasts({
        data,
        loading: false,
      });
    }
  }, []);
  return (
    <>
      {forecasts === undefined ? (
        <div>
          <h1>Error occured</h1>
          <h2>{error}</h2>
        </div>
      ) : (
        <Stack>
          <Stack direction="horizontal" className="ms-auto">
            <Button onClick={refresh} disabled={forecasts.loading}>
              {forecasts.loading ? "Loading" : "Refresh"}
            </Button>
          </Stack>
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
        </Stack>
      )}
    </>
  );
}
