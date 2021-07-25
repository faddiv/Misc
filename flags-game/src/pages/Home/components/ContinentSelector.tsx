import { ChangeEvent, FunctionComponent, memo, useCallback } from "react";
import Row from "react-bootstrap/Row";
import Col from "react-bootstrap/Col";
import Form from "react-bootstrap/Form";
import { Continent, continents } from "../../../flagsService/continentList";

interface ContinentSelectorProps {
  className: string;
  continent: Continent | null;
  onContinentSelected: (value: Continent | null) => void;
}

export const ContinentSelector: FunctionComponent<ContinentSelectorProps> = memo(({ className, continent, onContinentSelected }) => {
  const onChangeHandler = useCallback((evt: ChangeEvent<HTMLInputElement>) => {
    const value = evt.target.value;
    const newContinent = continents.find(e => e.name === value);
    onContinentSelected(newContinent?.name || null);
  }, [onContinentSelected]);

  return (
    <Row className={className}>
      {continents.map(e =>
        <Col as="label" xs={2} key={e.name} className={`continent-color-${e.name}`}>
          <Form.Control
            type="radio"
            name="continent"
            value={e.name}
            checked={e.name === continent}
            onChange={onChangeHandler}
            />
          {e.displayName}
        </Col>)}
    </Row>
  );
});
