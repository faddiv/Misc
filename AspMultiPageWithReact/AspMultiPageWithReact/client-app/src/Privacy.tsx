import { ChangeEvent, ReactNode, useCallback, useState } from "react";

export function Privacy() {
  const [row, setRow] = useState(100);
  const [coll, setColl] = useState(100);
  const colHandler = useCallback((evt: ChangeEvent<HTMLInputElement>) => {
    const newValue = parseInt(evt.target.value) || 1;
    setTimeout(() => {
      setColl(newValue);
    }, 0);
  }, []);
  const rowHandler = useCallback((evt: ChangeEvent<HTMLInputElement>) => {
    const newValue = parseInt(evt.target.value) || 1;
    setTimeout(() => {
      setRow(newValue);
    }, 0);
  }, []);
  return (
    <>
      <h1>Some title</h1>
      <p>This is a react generated page also</p>
      <div>
        <input type="number" value={coll} onChange={colHandler} />
      </div>
      <div>
        <input type="number" value={row} onChange={rowHandler} />
      </div>
      <table className="table">
        <thead>
          <tr>
            {repeat(coll, (index) => (
              <td key={index}>Header {index}</td>
            ))}
          </tr>
        </thead>
        <tbody>
          {repeat(row, (index1) => (
            <tr key={index1}>
              {repeat(coll, (index2) => (
                <td key={index2}>
                  Cell{index1}:{index2}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
function repeat(count: number, fn: (index: number) => ReactNode) {
  const arr: ReactNode[] = [];
  for (let index = 0; index < count; index++) {
    arr.push(fn(index));
  }
  return arr;
}
