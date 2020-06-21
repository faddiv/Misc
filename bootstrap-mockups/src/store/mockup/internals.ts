import { MockupElement } from "./models";

let id = 2;

export function createId() {
    return ++id;
}

export const initialState: MockupElement = {
    type: "Container",
    id: 1,
    children: []
};

export function addElement(root: MockupElement, element: MockupElement, parentId: number, index: number): MockupElement | undefined {
    if (!element)
        return;
    if (!root)
        return;
    if (root.id === parentId) {
        return modifyElementInPlace(root, index, 0, element);
    }
    for (let childIndex = 0; childIndex < root.children.length; childIndex++) {
        const child = root.children[childIndex];
        const newChild = addElement(child, element, parentId, index);
        if (typeof newChild === "undefined")
            continue;
        return modifyElementInPlace(root, childIndex, 1, newChild);
    }
    return;
}

export function removeElement(root: MockupElement, id: number): MockupElement | undefined {
    for (let childIndex = 0; childIndex < root.children.length; childIndex++) {
        const child = root.children[childIndex];
        if (child.id === id)
            return modifyElementInPlace(root, childIndex, 1);
        const newChild = removeElement(child, id);
        if (typeof newChild === "undefined")
            continue;
        return modifyElementInPlace(root, childIndex, 1, newChild);
    }
    return;
}

function modifyElementInPlace(root: MockupElement, index: number, remove: number, element?: MockupElement): MockupElement {
    var children = [...root.children];
    if (children.length <= index && typeof element !== "undefined") {
        children.push(element);
    } else {
        const elements: MockupElement[] = [];
        if (typeof element !== "undefined") {
            elements.push(element);
        }
        children.splice(index, remove, ...elements);
    }
    return {
        ...root,
        children
    };
}
