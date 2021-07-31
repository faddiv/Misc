import { FunctionComponent, useRef } from "react";
import { TheDisplay1, TheDisplay2 } from "./TheDisplay";

interface TheDisplayWrapperProps {

}

export const TheDisplayWrapper: FunctionComponent<TheDisplayWrapperProps> = () => {
  const rr = useRef(0);
  rr.current++;
  return (
    <div>
      <div>TheDisplayWrapper {rr.current}</div>
      <TheDisplay1 />
      <TheDisplay2 />
    </div>
  );
};
