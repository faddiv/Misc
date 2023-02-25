import { WeatherForecast } from "../../components/weatherForecast";

export interface FetchDataProps {
  data?: WeatherForecast[];
  error?: string;
}
