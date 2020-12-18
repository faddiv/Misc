import { FunctionComponent } from "react";
import { Context } from "./context";

interface DependencyInjectionProviderProps {

}

export const DependencyInjectionProvider: FunctionComponent<DependencyInjectionProviderProps> = ({ children }) => {
  return (
    <Context.Provider value={{}}>
      {children}
    </Context.Provider>
  );
};
