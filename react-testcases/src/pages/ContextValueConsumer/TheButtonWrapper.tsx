import { FunctionComponent, useRef } from "react";
import { TheButton1, TheButton2 } from "./TheButton";

interface TheButtonWrapperProps {

}

export const TheButtonWrapper: FunctionComponent<TheButtonWrapperProps> = () => {
    const rr = useRef(0);
    rr.current++;
    return (
        <div>
            <div>TheButtonWrapper: {rr.current}</div>
            <TheButton1 />
            <TheButton2 />
        </div>
    );
};
