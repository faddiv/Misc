import { FunctionComponent, useRef } from "react";
import { Col, Row } from "react-bootstrap";
import { TheButtonWrapper } from "./TheButtonWrapper";
import { TheContextProvider } from "./TheContextProvider";
import { TheDisplayWrapper } from "./TheDisplayWrapper";

// Since the Provider is extracted this component and it's direct children did not render only the Components using the Context.
// Also, for some unknown reason the react dev toll doesn't detect render of TheDisplay component.
export const ContextDrilling: FunctionComponent = () => {
    const rr = useRef(0);
    rr.current++;
    return (
        <TheContextProvider>
            <TheButtonWrapper />
            <TheDisplayWrapper />
            <Row><Col>Other thing. {rr.current}</Col></Row>
        </TheContextProvider>
    );
};
