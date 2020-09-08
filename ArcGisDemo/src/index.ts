// Map data
import { heatmapLayer, map } from './data/app';

// MapView
import MapView from 'esri/views/MapView';

// widget utils
import { initWidgets } from './widgets';

/**
 * Initialize application
 */
const view = new MapView({
  container: 'viewDiv',
  map,
});

heatmapLayer.when(() => {
  view.goTo(heatmapLayer.fullExtent);
});

view.when(initWidgets);
