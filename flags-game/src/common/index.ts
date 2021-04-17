import { ChangeEvent, useCallback } from "react";

export interface Action<T> {
  type: T
}

export function pickRandom(max: number) {
  return Math.floor(Math.random() * max);
}

export function useChangeToSetterHandler(setter: (value: string) => any) {
  return useCallback((evt: ChangeEvent<HTMLInputElement>) => {
    setter(evt.target.value);
  }, [setter]);
}
