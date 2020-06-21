import React from "react";
import { useDrop } from "react-dnd";
import { MockupElement, useMockupActions, MockupElementType } from "./store/mockup";

interface RecycleBinProps { }

export const RecycleBin: React.FunctionComponent<RecycleBinProps> = ({ ...rest }) => {
    const actions = useMockupActions();
    const mockupTypes: MockupElementType[] = ["Row", "Col", "Container"];
    const [, drop] = useDrop({
        accept: mockupTypes,
        drop(item: MockupElement, monitor) {
            if (item.id > 0) {
                actions.remove(item.id);
            }
        }
    })
    return (
        <div ref={drop}>Recycle bin</div>
    );
};
