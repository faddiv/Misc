import { flags } from "./flagList";
import { FlagChecked, PlayListWithFlag } from "./playList";
import { validateFlags } from "./validateFlags";
import { playListStore } from "./playListStore";

if (playListStore.isEmpty()) {
  // init
  playListStore.savePlayList({
    id: 0,
    name: "All flags",
    flags: flags.map(e => e.Pic)
  });
}

let list = generateList();

function generateList() {
  const list: PlayListWithFlag[] = [];
  for (const key of playListStore.getPlayListKeys()) {
    const element = playListStore.getPlayList(key);
    const invalids = validateFlags(element.flags);
    list.push({
      id: element.id,
      name: element.name,
      invalids: invalids,
      flags: expand(element.flags)
    });
  }
  return list;

}

export function expand(flagNames: string[]) {
  const result: FlagChecked[] = [];
  for (let index = 0; index < flags.length; index++) {
    const element = flags[index];
    result.push({
      Eng: element.Eng,
      Hun: element.Hun,
      Pic: element.Pic,
      checked: flagNames.indexOf(element.Pic) !== -1
    });
  }
  return result;
}

export const tempBackend = {
  list
};
