import { FunctionComponent } from "react";
import { Col, Row } from "react-bootstrap";
import { flags } from "../../flagsService/flagList";
import { useGame } from "../../flagsService/useGame";
import { useStaticFlagList } from "../../flagsService/usePlayLists";
import { CountryAutocomplete } from "./CountryAutocomplete";
import { SelectButton } from "./selectButton";

interface HomePageProps {

}

export const HomePage: FunctionComponent<HomePageProps> = () => {
  const { plList, selected, selectPlay } = useStaticFlagList();
  const { input, setInput, guess, pickNew, flagInfo, gameState } = useGame(selected);

  return (
    <>
      <Row>
        <Col xs={{ span: 12, order: 2 }} md={{ span: 2, order: 1 }}>
          <br className="d-block d-md-none" />
          <Row>
            {plList.map(playList =>
              <Col xs={6} md={12} key={playList.id}><SelectButton playList={playList} onSelected={selectPlay} selected={selected?.id === playList.id} /></Col>
            )}
          </Row>
        </Col>
        <Col xs={{ span: 12, order: 1 }} md={{ span: 10, order: 2 }}>
          <br />
          Correct: {gameState.numOfCorrect} Wrong: {gameState.numOfWrong}
          <br />
          Current flag correct: {gameState.flagsState[gameState.lastFlag]?.correct} Wrong: {gameState.flagsState[gameState.lastFlag]?.wrong}
          <br />
          <img src={`Flags/${flagInfo.Pic}`} className="country-flag" alt="logo" />
          <p>Guess!</p>
          <form onSubmit={guess}>
            <CountryAutocomplete value={input} onValueChanged={setInput} flags={flags} />
            <button type="submit">Check</button>
            <button type="button" onClick={pickNew}>Pick</button>
          </form>
        </Col>
      </Row>
    </>
  );
};
