import { FunctionComponent, memo, useEffect } from "react";
import { Col, Row } from "react-bootstrap";
import { PlayList } from "../../../flagsService/playList";
import { useStaticFlagList } from "../../../flagsService/usePlayLists";
import { SelectButton } from "../../../ViewComponents";

interface PlaySelectorProps {
  selectedChanged: (selected: PlayList | null) => void;
}

export const PlaySelector: FunctionComponent<PlaySelectorProps> = memo(({selectedChanged}) => {
  const { plList, selected, selectPlay } = useStaticFlagList();

  useEffect(() => {
    selectedChanged(selected);
  }, [selectedChanged, selected]);

  return (
    <Row>
      {plList.map(playList =>
        <Col xs={6} md={12} key={playList.id}>
          <SelectButton playList={playList} onSelected={selectPlay} selected={selected?.id === playList.id} />
        </Col>
      )}
    </Row>
  );
});
