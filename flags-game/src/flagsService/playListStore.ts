import { PlayList } from "./playList";

if (typeof (localStorage) === "undefined") {
  alert("Local storage not supported.");
}

const playListPrefix = "PlayList_";
const idGeneratorKey = "id_generator";

function getNextId() {

  var idText = localStorage.getItem(idGeneratorKey);
  let id = idText === null ? 0 : parseInt(idText);
  id++;
  localStorage.setItem(idGeneratorKey, id.toString());
  return id;
}

function getPlayListKeys() {
  const playLists: number[] = [];
  for (let index = 0; index < localStorage.length; index++) {
    const element = localStorage.key(index);
    if (element !== null && element.startsWith(playListPrefix)) {
      playLists.push(parseInt(element.substring(playListPrefix.length)));
    }
  }
  return playLists;
}

function getPlayList(key: number): PlayList {
  const json = localStorage.getItem(playListPrefix + key);
  if (json === null) {
    throw new Error("Playlist doesn't exists: " + key);
  }
  const playList: PlayList = JSON.parse(json);
  return playList;
}

function savePlayList(playList: PlayList) {
  const id = playList.id === 0 ? getNextId() : playList.id;
  const json = JSON.stringify({ ...playList, id });
  localStorage.setItem(playListPrefix + id.toString(), json);
  return id;
}

function isEmpty() {
  return localStorage.getItem(idGeneratorKey) === null;
}

export const playListStore = {
  getPlayListKeys,
  getPlayList,
  savePlayList,
  isEmpty
}
