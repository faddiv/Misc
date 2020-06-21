import React from "react";
import { MockupElement } from "../store/mockup";
import { Container } from "reactstrap";
import { RowPlaceholder } from "./RowPlaceholder";

interface  DraggableContainerProps { 
    mockupElement: MockupElement
}

export const DraggableContainer: React.FunctionComponent< DraggableContainerProps> = ({ mockupElement, ...rest }) => {
    let children = rest.children as JSX.Element[];
    const empty = !mockupElement.children.length;
    let renderable: any = undefined;
    if (!empty) {
        renderable = children.map((e, index) => [e, <RowPlaceholder key={`plc ${index + 1}`} onlyOne={false} dropIndex={index + 1} parentId={mockupElement.id} />]);
    }
    return (
        <Container fluid={true}>
            <RowPlaceholder key="plc 0" onlyOne={empty} dropIndex={0} parentId={mockupElement.id} />
            {renderable}
          </Container>
    );
};

DraggableContainer.defaultProps = { };