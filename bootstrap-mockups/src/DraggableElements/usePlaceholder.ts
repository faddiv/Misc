import { useDrop } from 'react-dnd'
import { MockupElement, useMockupActions, MockupElementType } from "../store/mockup";

export function usePlaceholder(accept: MockupElementType, parentId: number, dropIndex: number) {
    const actions = useMockupActions();
    return useDrop({
        accept: accept,
        collect: (monitor) => (
            {
                isOver: monitor.isOver()
            }
        ),
        drop(item: MockupElement, monitor) {
            if(item.id === 0) {
                actions.add(item.type, parentId, dropIndex);
            } else {
                console.log("implement move");
            }
        }
    });
}