import CSVLayer from 'esri/layers/CSVLayer';
import Map from 'esri/Map';
import HeatmapRenderer from "esri/renderers/HeatmapRenderer";

const renderer = new HeatmapRenderer({
  maxPixelIntensity: 25,
  minPixelIntensity: 0
});

export const heatmapLayer = new CSVLayer({
  url: "https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/2.5_week.csv",
  title: "Magnitude 2.5+ earthquakes from the last week",
  renderer: renderer,
  opacity: 0.9
});

export const map = new Map({
  basemap: "topo-vector",
  layers: [heatmapLayer]
});
