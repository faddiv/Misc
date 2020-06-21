import React from "react";
import classNames from "classnames";
import { usePlaceholder } from "./usePlaceholder";

interface ColPlaceholderProps {
    parentId: number;
    dropIndex: number;
    onlyOne: boolean;
}

export const ColPlaceholder: React.FunctionComponent<ColPlaceholderProps> = ({ parentId, onlyOne, dropIndex, ...rest }) => {
    const [{ isOver }, drop] = usePlaceholder("Col", parentId, dropIndex);

    return (
        <div
            ref={drop}
            className={classNames("col-placeholder", { hovered: isOver, "only-one": onlyOne })}
        >+Col</div>
    );
};
