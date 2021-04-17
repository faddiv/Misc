import { FunctionComponent, useCallback } from "react";
import { Button } from "react-bootstrap";
import { PlayListWithFlag } from "../../flagsService/playList";

interface SelectButtonProps {
  playList: PlayListWithFlag;
  selected: boolean;
  onSelected: (selectedId: number) => void;
}

export const SelectButton: FunctionComponent<SelectButtonProps> = ({ playList, onSelected, selected }) => {
  const selectHandler = useCallback(() => {
    if (selected)
      return;
    onSelected(playList.id);
  }, [playList, selected, onSelected]);
  return (
    <Button key={playList.id} className="w-100" variant={selected ? "primary" : "info"} onClick={selectHandler}>{playList.name}</Button>
  );
};
