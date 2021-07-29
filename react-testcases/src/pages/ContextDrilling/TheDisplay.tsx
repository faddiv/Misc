import { FunctionComponent, useContext } from "react";
import { TheContext } from "./TheContext";

export const TheDisplay: FunctionComponent = () => {
    const { counter } = useContext(TheContext);
    return (
        <div>Counter: {counter}</div>
    );
};
