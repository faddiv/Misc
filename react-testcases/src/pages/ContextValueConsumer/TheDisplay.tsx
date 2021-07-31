import { useRef } from "react";
import { withLogicValues } from "./LogicProvider";

function TheDisplayInner1({ count1 }: { count1: number }) {
  const rr = useRef(0);
  rr.current++;
  return (
    <div>TheDisplayInner1: {rr.current}; Count1: {count1}</div>
  );
};

export const TheDisplay1 = withLogicValues("count1")(TheDisplayInner1);

function TheDisplayInner2({ count2 }: { count2: number }) {
  const rr = useRef(0);
  rr.current++;
  return (
    <div>TheDisplayInner2: {rr.current}; Count2: {count2}</div>
  );
};

export const TheDisplay2 = withLogicValues("count2")(TheDisplayInner2);
