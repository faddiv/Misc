import React from "react";
import classNames from "classnames";
import { ColPlaceholder } from "./ColPlaceholder";
import { MockupElement } from "../store/mockup";
import { useDrag } from "react-dnd";

interface DraggableRowProps {
    mockupElement: MockupElement
}

export const DraggableRow: React.FunctionComponent<DraggableRowProps> = ({ mockupElement, ...rest }) => {
    const [, drag] = useDrag({
        item: mockupElement,
    });
    const isToolbarElement = mockupElement.id === 0;
    let children = rest.children as JSX.Element[];
    const empty = !mockupElement.children.length;
    let renderable: any = undefined;
    if (!empty) {
        renderable = children.map((e, index) => [e, <ColPlaceholder key={`plc ${index + 1}`} onlyOne={false} dropIndex={index + 1} parentId={mockupElement.id} />]);
    }
    return (
        <div
            className={classNames("row")}
            ref={drag}
        >
            {!isToolbarElement && <ColPlaceholder key="plc 0" onlyOne={empty} dropIndex={0} parentId={mockupElement.id} />}
            {renderable || "Row 0"}
        </div>
    );
};

DraggableRow.defaultProps = {};