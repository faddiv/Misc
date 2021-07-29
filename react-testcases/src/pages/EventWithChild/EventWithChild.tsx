import { FunctionComponent, useCallback, useState } from "react";
import { Button, Col } from "react-bootstrap";
import { ChildManipulator } from "./ChildManipulator";

// if the inner event fired then only the ChildManipulator and its descendant do a rerender. This component is unchanged.
// but if the outer event fired the ChildManipulator also rerendered despite the memo because the child changed.
export const EventWithChild: FunctionComponent = () => {
    const [count, setCount] = useState(0);
    const increment = useCallback(() => {
        setCount(n => n + 1);
    }, []);
    return (
        <div>
            <Button onClick={increment}>Count outer</Button>
            <ChildManipulator>
                {count2 => (<>
                    <Col>Outer: {count}</Col>
                    <Col>inner: {count2}</Col>
                </>)}
            </ChildManipulator>
        </div>
    );
};
