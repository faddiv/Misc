import React from "react";
import { useDrag } from "react-dnd";
import classNames from "classnames";
import { MockupElement } from "../store/mockup";

interface DraggableColProps {
    mockupElement: MockupElement
}

export const DraggableCol: React.FunctionComponent<DraggableColProps> = ({ mockupElement, ...rest }) => {
    const [, drag] = useDrag({
        item: mockupElement,
    });
    const empty = !mockupElement.children.length;
    const renderable = empty ? undefined : rest.children;
    return (
        <div
            className={classNames("col")}
            ref={drag}
        >
            {renderable || "Column " + mockupElement.id}
        </div>
    );
};

DraggableCol.defaultProps = {};