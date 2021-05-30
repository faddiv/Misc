import { ChangeEvent, useCallback } from "react";

export interface Action<T> {
  type: T
}

export function pickRandom(max: number) {
  return Math.random() * max;
}

export function findValIndex(val: number, list: number[], start: number, end: number): number {
  if (start === end) {
    return 1 + Math.min(start, list.length - 1);
  }
  const middle = start + Math.trunc((end - start) / 2);
  if (list[middle] < val) {
    if (middle + 1 === end) {
      return Math.min(end, list.length - 1);
    }
    return findValIndex(val, list, middle, end);
  } else if (middle === 0) {
    return 0;
  } else {
    return findValIndex(val, list, start, middle);
  }
}

export function useChangeToSetterHandler(setter: (value: string) => any) {
  return useCallback((evt: ChangeEvent<HTMLInputElement>) => {
    setter(evt.target.value);
  }, [setter]);
}
