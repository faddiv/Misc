import { useCallback, useState } from "react";

export { Page }

function Page() {
  const [counter, setCounter] = useState(0);
  const incrementCounter = useCallback(() => {
    setCounter(c => c + 1);
  },[]);
  return (
    <div>
      <h1>Counter</h1>
      <p>This is a simple example of a React component.</p>
      <p aria-live="polite">Current count: <strong>{counter}</strong></p>
      <button className="btn btn-primary" onClick={incrementCounter}>Increment</button>
    </div>
  );
}
