import { FunctionComponent } from "react";
import { Button, Col, Form, InputGroup, Row } from "react-bootstrap";
import { AlertBox } from "../../alerts";
import { flags } from "../../flagsService/flagList";
import { useGame } from "../../flagsService/useGame";
import { useStaticFlagList } from "../../flagsService/usePlayLists";
import { CountryAutocomplete } from "./CountryAutocomplete";
import { SelectButton } from "../../ViewComponents";

interface HomePageProps {

}

export const HomePage: FunctionComponent<HomePageProps> = () => {
  const { plList, selected, selectPlay } = useStaticFlagList();
  const { input, setInput, guess, pickNew, flagInfo, gameState, guessStep } = useGame(selected);

  return (
    <>
      <Row>
        <Col xs={{ span: 12, order: 2 }} md={{ span: 2, order: 1 }}>
          <br className="d-block d-md-none" />
          <Row>
            {plList.map(playList =>
              <Col xs={6} md={12} key={playList.id}>
                <SelectButton playList={playList} onSelected={selectPlay} selected={selected?.id === playList.id} />
              </Col>
            )}
          </Row>
        </Col>
        <Col xs={{ span: 12, order: 1 }} md={{ span: 10, order: 2 }}>
          <div className="alert-placeholder">
            <AlertBox />
          </div>
          <div className="pic-placeholder">
            <img src={`Flags/${flagInfo.Pic}`} className="country-flag border" alt="logo" />
          </div>
          <div className="input-placeholder">
            <Form onSubmit={guess} inline>
              <InputGroup>
                <CountryAutocomplete value={input} onValueChanged={setInput} flags={flags} />
                <InputGroup.Append>
                  <Button type="submit" variant="primary">{guessStep === 1 ? "Next" : "Check"} </Button>
                  <Button type="button" variant="secondary" onClick={pickNew}>Pick</Button>
                </InputGroup.Append>
              </InputGroup>
            </Form>
          </div>
          <br />
          Correct: {gameState.numOfCorrect} Wrong: {gameState.numOfWrong}
          <br />
          Current flag correct: {gameState.flagsState[gameState.lastFlag]?.correct} Wrong: {gameState.flagsState[gameState.lastFlag]?.wrong}
          <br />
        </Col>
      </Row>
    </>
  );
};
