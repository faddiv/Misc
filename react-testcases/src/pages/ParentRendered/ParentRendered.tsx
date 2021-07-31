import { FunctionComponent, memo, PropsWithChildren, useCallback, useRef, useState } from "react";
import { Button, Col, Row } from "react-bootstrap";

interface ParentRenderedProps {

}

export const ParentRendered: FunctionComponent<ParentRenderedProps> = () => {
  const [counter1, setCounter1] = useState(0);
  const fun1 = useCallback(() => {
    setCounter1(c => c + 1);
  }, []);
  const rc = useRef(0);
  const rc2 = useRef(0);
  rc.current++;
  return (
    <>
      <Row>
        <Button1 onClick={fun1} />
        <Col>Counter: {counter1} RC:{rc.current}</Col>
        <Child1 cn={1} num={1} />
      </Row>

      <Row>
        <Child2 cn={4} num={1} />
        <Child2 cn={5} num={1} ></Child2>
        <Child2 cn={6} num={1} >Inner counter: {counter1}</Child2>
      </Row>

      <Row>
        <Child3 cn={4} num={1} />
        <Child3 cn={5} num={1} ></Child3>
        <Child3 cn={6} num={1} >Inner counter: {counter1}</Child3>
      </Row>
      <ChangingComponent>
        <Wrapper><Col>Counter: {counter1} RC2: {++rc2.current}</Col></Wrapper>
      </ChangingComponent>
    </>
  );
};

function Button1({ onClick }: { onClick: () => void }) {
  return (<Button onClick={onClick}>Increment num 1</Button>);
}

function Child1({ num, cn, children }: PropsWithChildren<{ num: number, cn: number }>) {
  const rc = useRef(0);
  rc.current++;
  return (<Col>Child{cn}: {num} ({children}) RC: {rc.current}</Col>);
}
const Child2 = memo(Child1);

const Child3 = memo(({ num, cn, children }: PropsWithChildren<{ num: number, cn: number }>) => {
  const rc = useRef(0);
  rc.current++;
  return (
    <Col>
      <Row>
        <Col>RC:{rc.current}</Col>
        <Child4>{children}</Child4>
      </Row>
    </Col>);
});


const Child4 = memo(({ children }: PropsWithChildren<{}>) => {
  return (<Col>{children}</Col>);
});

const ChangingComponent = memo(({ children }: PropsWithChildren<{}>) => {
  const [counter1, setCounter1] = useState(0);
  const fun1 = useCallback(() => {
    setCounter1(c => c + 1);
  }, []);
  const rc = useRef(0);
  rc.current++;
  return (
    <Row>
      <Button1 onClick={fun1} />
      <Col>Counter: {counter1} RC: {rc.current}</Col>
      <Col>{children}</Col>
    </Row>
  );
});

const Wrapper = ({ children }: PropsWithChildren<{}>) => {
  const rc = useRef(0);
  rc.current++;
  return (<div><div>Wrapper: {rc.current}</div><div>{children}</div></div>)
}
