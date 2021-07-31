import { FunctionComponent, useRef } from "react";
import { Col, Row } from "react-bootstrap";
import { LogicProvider } from "./LogicProvider";
import { TheButtonWrapper } from "./TheButtonWrapper";
import { TheDisplayWrapper } from "./TheDisplayWrapper";

export const ContextValueConsumerExample: FunctionComponent = () => {
  const rr = useRef(0);
  rr.current++;
  return (
    <LogicProvider>
      <TheButtonWrapper />
      <TheDisplayWrapper />
      <Row><Col>Other thing. {rr.current}</Col></Row>
    </LogicProvider>
  );
};
