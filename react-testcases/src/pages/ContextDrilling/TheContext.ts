import { createContext } from "react";

export interface ContextValue {
    counter: number;
    setCounter: (p: (n: number) => number) => void;
}

export const TheContext = createContext<ContextValue>(null as any);