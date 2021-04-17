import { FunctionComponent, useCallback } from "react";
import { Button, Card, Col } from "react-bootstrap";
import { FlagChecked } from "../../flagsService/playList";

interface SelectableFlagProps {
  flag: FlagChecked;
  onFlagChecked: (flag: FlagChecked) => void;
}

export const SelectableFlag: FunctionComponent<SelectableFlagProps> = ({ flag, onFlagChecked }) => {
  const handleChecked = useCallback(() => {
    onFlagChecked(flag);
  }, [flag, onFlagChecked]);
  return (
    <Col xs={6} md={4} lg={3}>
      <Card>
        <Card.Img variant="top" src={`Flags/${flag.Pic}`} alt="logo" loading="lazy" />
        <Card.Body>
          <Card.Title>{flag.Hun}</Card.Title>
          <Button variant={flag.checked ? "success" : "danger"}
            onClick={handleChecked}>
            {flag.checked ? "Selected" : "Unselected"}
          </Button>
        </Card.Body>
      </Card>
    </Col>
  );
};
