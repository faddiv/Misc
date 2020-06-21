import React from "react";
import { MockupElement } from "../store/mockup";
import { DraggableRow } from "./DraggableRow";
import { DraggableCol } from "./DraggableCol";
import { DraggableContainer } from "./DraggableContainer";

interface DraggableElementRendererProps {
    mockupElement: MockupElement
}

export const DraggableElementRenderer: React.FunctionComponent<DraggableElementRendererProps> = ({ mockupElement, ...rest }) => {
    var children = mockupElement.children.map(e => <DraggableElementRenderer key={`elm ${e.id}`} mockupElement={e} />);
    switch (mockupElement.type) {
        case "Container":
            return (
                <DraggableContainer mockupElement={mockupElement}>{children}</DraggableContainer>
            );
        case "Row":
            return (
                <DraggableRow mockupElement={mockupElement}>{children}</DraggableRow>
            );
        case "Col":
            return (
                <DraggableCol mockupElement={mockupElement}>{children}</DraggableCol>
            );
        default:
            return <strong>Unknown element</strong>
    }
};

DraggableElementRenderer.defaultProps = {};