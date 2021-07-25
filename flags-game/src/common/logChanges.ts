import { useRef } from "react";

export function useChangeTracker(tractedProps: any[]) {
  const prev = useRef<any[] | null>(null);
  const count = useRef<number>(0);
  const prevProps = prev.current;
  if (prevProps === null) {
    console.log("initial render:", tractedProps, "count:", count.current);
  } else {
    if (tractedProps.length !== prevProps.length) {
      throw new Error("Count of tracked props changed")
    }
    const changes = [];
    for (let index = 0; index < prevProps.length; index++) {
      const prevElement = prevProps[index];
      const nextElement = tractedProps[index];
      if (prevElement === nextElement) {
        changes.push("unchanged");
      } else {
        changes.push({ from: prevElement, to: nextElement });
      }
    }
    console.log("re-render:", changes, "count:", count.current);
  }
  count.current += 1;
  prev.current = tractedProps;
}
