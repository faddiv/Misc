import { extTable } from "../common/extendedStore";
import { PlayList } from "./playList";

const table = extTable<PlayList>("PlayList");

// update logic
(() => {
  const playListPrefix = "PlayList_";
  const idGeneratorKey = "id_generator";
  const id_generator = localStorage.getItem(idGeneratorKey);
  if (id_generator === null)
    return;
  for (let index = 0; index < localStorage.length; index++) {
    var key = localStorage.key(index);
    if (key === null)
      continue;
    if (!key.startsWith(playListPrefix))
      continue;
    const json = localStorage.getItem(key);
    if (json === null)
      continue;
    const element = JSON.parse(json) as PlayList;
    element.id = 0;
    table.saveElement(element);
    localStorage.removeItem(key);
  }
  localStorage.removeItem(idGeneratorKey);
})();

function getPlayListKeys() {
  return table.getKeys();
}

function getPlayList(key: number) {
  return table.getElementByKey(key);
}

function savePlayList(playList: PlayList) {
  return table.saveElement(playList);
}

function isEmpty() {
  return table.isEmpty();
}

export const playListStore = {
  getPlayListKeys,
  getPlayList,
  savePlayList,
  isEmpty
}
