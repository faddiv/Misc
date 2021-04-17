import { FunctionComponent, useCallback } from "react";
import { Button } from "react-bootstrap";
import { PlayListWithFlag } from "../../flagsService/playList";

interface PlSelectElementProps {
  playList: PlayListWithFlag;
  selected: boolean;
  onPlayListSelected: (playList: PlayListWithFlag) => void;
}

export const PlSelectElement: FunctionComponent<PlSelectElementProps> = ({ playList, selected, onPlayListSelected }) => {
  const selectHandler = useCallback(() => {
    if (selected)
      return;
    onPlayListSelected(playList);
  }, [selected, onPlayListSelected, playList]);
  return (
    <Button key={playList.name} variant={selected ? "primary" : "info"} className="w-100" onClick={selectHandler}>
      {playList.name}
    </Button>
  );
};
