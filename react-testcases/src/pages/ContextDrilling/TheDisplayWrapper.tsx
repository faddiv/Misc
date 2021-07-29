import { FunctionComponent, memo, useRef } from "react";
import { TheDisplay } from "./TheDisplay";

interface TheDisplayWrapperProps {

}

export const TheDisplayWrapper: FunctionComponent<TheDisplayWrapperProps> = () => {
    const rr = useRef(0);
    rr.current++;
    return (
        <div>
            <TheDisplay />
            <div>Before display {rr.current}</div>
        </div>
    );
};
