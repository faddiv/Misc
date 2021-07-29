import { FunctionComponent, memo, ReactNode, useCallback, useState } from "react";
import { Button, Col, Row } from "react-bootstrap";

interface ChildManipulatorProps {
    children: (n: number) => ReactNode;
}

export const ChildManipulator: FunctionComponent<ChildManipulatorProps> = memo(({ children }) => {
    const [count, setCount] = useState(0);
    const increment = useCallback(() => {
        setCount(n => n + 1);
    }, []);
    return (
        <>
            <Row>
                <Col>
                    <Button onClick={increment}>Count inner</Button>
                </Col>
            </Row>
            <Row>
                {children(count)}
            </Row>
        </>
    );
});
