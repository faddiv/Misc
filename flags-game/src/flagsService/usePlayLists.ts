import { ChangeEvent, FormEvent, useCallback, useEffect, useState } from "react";
import { FlagChecked, PlayList, PlayListWithFlag } from "./playList";
import { playListStore } from "./playListStore";
import { expand, tempBackend } from "./templPlayLists";

function extract(flags: FlagChecked[]): string[] {
  const result: string[] = [];
  for (const flag of flags) {
    if (flag.checked) {
      result.push(flag.Pic);
    }
  }
  return result;
}

export function usePlayList() {
  const [plList, setPlList] = useState<PlayListWithFlag[]>([]);
  const [selected, setSelected] = useState<PlayListWithFlag | null>(null);
  const [changed, setChanged] = useState(false);
  useEffect(() => {
    if (plList.length === 0) {
      setPlList(tempBackend.list);
      setSelected({
        ...tempBackend.list[0],
        flags: [...tempBackend.list[0].flags]
      });
      setChanged(false);
    }
  }, [plList]);

  const flagCheckedHandler = useCallback((flag: FlagChecked) => {
    if (selected === null)
      return;
    var newFlag: FlagChecked = { ...flag, checked: !flag.checked };
    var index = selected.flags.indexOf(flag);
    var newSelected: PlayListWithFlag = { ...selected, flags: [...selected.flags.slice(0, index), newFlag, ...selected.flags.slice(index + 1)] };
    setSelected(newSelected);
    setChanged(true);
  }, [selected]);

  const nameChanged = useCallback((evt: ChangeEvent<HTMLInputElement>) => {
    if (selected === null)
      return;
    setSelected({ ...selected, name: evt.target.value });
    setChanged(true);
  }, [selected]);

  const newElementHandler = useCallback(() => {
    setSelected({
      id: 0,
      name: "",
      flags: expand([])
    });
  }, []);

  const selectElement = useCallback((selectedId: number) => {
    var selectedElement = plList.find(e => e.id === selectedId);
    if (selectedElement) {
      setSelected(selectedElement);
    }
  }, [plList]);
  const saveHandler = useCallback((evt: FormEvent<HTMLFormElement>) => {
    if (selected === null)
      return;
    evt.preventDefault();
    let newList: PlayListWithFlag[];
    const newSelect: typeof selected = {
      ...selected,
      invalids: undefined
    };
    if (newSelect.id > 0) {
      const index = plList.findIndex(e => e.id === newSelect.id);
      newList = [...plList.slice(0, index), newSelect, ...plList.slice(index + 1)];
    } else {
      newList = [...plList, newSelect];
    }
    newSelect.id = playListStore.savePlayList({
      id: newSelect.id,
      name: newSelect.name,
      flags: extract(newSelect.flags)
    });
    tempBackend.list = newList;
    setPlList(newList);
    setSelected(newSelect);
    setChanged(false);
  }, [selected, plList]);

  return { plList, selected, flagCheckedHandler, changed, nameChanged, saveHandler, newElementHandler, selectElement };
}

const empty: typeof tempBackend.list = [];
export function useStaticFlagList() {
  const [plList, setPlList] = useState<typeof tempBackend.list>(empty);
  const [selected, setSelected] = useState<PlayList | null>(null);
  useEffect(() => {
    if (plList === empty) {
      setPlList(tempBackend.list);
      setSelected(playListStore.getPlayList(tempBackend.list[0].id));
    }
  }, [plList]);
  const selectPlay = useCallback((selectedId: number) => {
    setSelected(playListStore.getPlayList(selectedId));
  }, []);
  return { plList, selected, selectPlay };
}
