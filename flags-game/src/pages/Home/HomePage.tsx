import { FunctionComponent, useState } from "react";
import Button from "react-bootstrap/Button";
import Col from "react-bootstrap/Col";
import Form from "react-bootstrap/Form";
import InputGroup from "react-bootstrap/InputGroup";
import Row from "react-bootstrap/Row";
import { AlertBox } from "../../alerts";
import { flags } from "../../flagsService/flagList";
import { useGame } from "../../flagsService/useGame";
import { CountryAutocomplete } from "./components/CountryAutocomplete";
import { ContinentSelector } from "./components/ContinentSelector";
import { PlaySelector } from "./components/PlaySelector";
import { PlayList } from "../../flagsService/playList";
import { FlagView } from "./components/FlagView";

interface HomePageProps {

}

export const HomePage: FunctionComponent<HomePageProps> = () => {

  const [selected, setSelected] = useState<PlayList | null>(null);
  const { input, setInput, guess, pickNew, flagInfo, gameState, guessStep, continent, setContinent } = useGame(selected);

  return (
    <>
      <Row>
        <Col xs={{ span: 12, order: 2 }} md={{ span: 2, order: 1 }}>
          <br className="d-block d-md-none" />
          <PlaySelector selectedChanged={setSelected} />
        </Col>
        <Col xs={{ span: 12, order: 1 }} md={{ span: 10, order: 2 }}>
          <div className="alert-placeholder">
            <AlertBox />
          </div>
          <div className="pic-placeholder">
            <FlagView flagInfo={flagInfo} />
          </div>
          <Form onSubmit={guess}>
            <InputGroup className="mt-1">
              <CountryAutocomplete value={input} onValueChanged={setInput} flags={flags} />
              <InputGroup.Append>
                <Button type="submit" variant="primary">{guessStep === 1 ? "Next" : "Check"} </Button>
                <Button type="button" variant="secondary" onClick={pickNew}>Pick</Button>
              </InputGroup.Append>
            </InputGroup>
            <ContinentSelector className="mt-1" continent={continent} onContinentSelected={setContinent} />
          </Form>
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
