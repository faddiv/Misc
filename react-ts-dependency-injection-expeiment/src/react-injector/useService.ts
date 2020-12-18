import { useContext } from "react";
import { Context } from "./context";

export function useService(name: string) {
  return useContext(Context);
}
