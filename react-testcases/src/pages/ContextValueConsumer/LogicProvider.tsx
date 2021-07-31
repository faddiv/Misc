import { createContext, FunctionComponent, useCallback, useState } from "react";
import { withPartialContext } from "./withPartialContext";

type ContextModel = { count1: number, count2: number, increment1: () => void, increment2: () => void };

const LogicContext = createContext<ContextModel>(null as any);

interface LogicProviderProps {

}

export const LogicProvider: FunctionComponent<LogicProviderProps> = ({ children }) => {

  const [count1, setCount1] = useState(0);
  const [count2, setCount2] = useState(0);
  const increment1 = useCallback(() => {
    setCount1(n => n + 1);
  }, []);
  const increment2 = useCallback(() => {
    setCount2(n => n + 1);
  }, []);
  return (
    <LogicContext.Provider value={{ count1, count2, increment1, increment2 }}>
      {children}
    </LogicContext.Provider>
  );
};

export function withLogicValues<CV extends Extract<keyof ContextModel, string>>(...injectable: CV[]) {
  return withPartialContext(LogicContext, true, ...injectable);
}
