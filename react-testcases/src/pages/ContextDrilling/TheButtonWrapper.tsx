import { FunctionComponent, memo, useRef } from "react";
import { TheButton } from "./TheButton";

interface TheButtonWrapperProps {

}

export const TheButtonWrapper: FunctionComponent<TheButtonWrapperProps> = () => {
    const rr = useRef(0);
    rr.current++;
    return (
        <div>
            <div>Before the button {rr.current}</div>
            <TheButton />
        </div>
    );
};
