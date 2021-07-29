import { FunctionComponent, useMemo, useState } from "react";
import { TheContext } from "./TheContext";

interface TheContextProviderProps {

}

export const TheContextProvider: FunctionComponent<TheContextProviderProps> = ({ children }) => {
    const [counter, setCounter] = useState(0);
    const value = useMemo(() => ({
        counter,
        setCounter
    }), [counter])
    return (
        <TheContext.Provider value={value}>{children}</TheContext.Provider>
    );
};
