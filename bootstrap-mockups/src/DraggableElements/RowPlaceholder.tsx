import React from "react";
import classNames from "classnames";
import { usePlaceholder } from "./usePlaceholder";

interface RowPlaceholderProps {
    parentId: number;
    dropIndex: number;
    onlyOne: boolean;
}

export const RowPlaceholder: React.FunctionComponent<RowPlaceholderProps> = ({ parentId, onlyOne, dropIndex, ...rest }) => {
    const [{ isOver }, drop] = usePlaceholder("Row", parentId, dropIndex);

    return (
        <div
            ref={drop}
            className={classNames("row-placeholder", { hovered: isOver, "only-one": onlyOne })}
        >+Row</div>
    );
};
