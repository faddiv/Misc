export type MockupElementType = "Container" | "Row" | "Col";

export interface MockupElement {
    type: MockupElementType;
    id: number;
    children: MockupElement[];
}
