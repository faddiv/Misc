import { FunctionComponent, useCallback, useContext, useRef } from "react";
import { Button } from "react-bootstrap";
import { TheContext } from "./TheContext";

interface TheButtonProps {

}

export const TheButton: FunctionComponent<TheButtonProps> = () => {
    const rr = useRef(0);
    rr.current++;
    const { setCounter } = useContext(TheContext);
    const increment = useCallback(() => {
        setCounter((n) => (n + 1));
    }, [setCounter]);
    return (
        <Button onClick={increment}>Increment {rr.current}</Button>
    );
};
