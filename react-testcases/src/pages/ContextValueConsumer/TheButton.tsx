import { useRef } from "react";
import { Button } from "react-bootstrap";
import { withLogicValues } from "./LogicProvider";

function InnerButton1({ increment1 }: { increment1: () => void }) {
  const rr = useRef(0);
  rr.current++;
  return (
    <Button onClick={increment1} style={{ margin: "5px" }}>InnerButton1 {rr.current} Increment1</Button>
  );
}

export const TheButton1 = withLogicValues("increment1")(InnerButton1);


function InnerButton2({ increment2 }: { increment2: () => void }) {
  const rr = useRef(0);
  rr.current++;
  return (
    <Button onClick={increment2} style={{ margin: "5px" }}>InnerButton2 {rr.current} Increment2</Button>
  );
}

export const TheButton2 = withLogicValues("increment2")(InnerButton2);
