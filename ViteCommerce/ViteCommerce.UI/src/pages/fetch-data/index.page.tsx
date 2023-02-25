import { useState } from "react";
import { FetchDataProps } from "./FetchDataProps";

export function Page({ data, error }: FetchDataProps) {
  const [forecasts, setForecasts] = useState(data);
  return (
    <>
      {forecasts === undefined ? (
        <div>
          <h1>Error occured</h1>
          <h2>{error}</h2>
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
            {forecasts.map((forecast) => (
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
    </>
  );
}
