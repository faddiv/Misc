import { mockupReducers } from "./reducers";
import { MockupActions } from "./actions";
import { MockupElementType, MockupElement } from "./models";

let idGen = 1;
function createMockupData(args: { type?: MockupElementType, id?: number, children?: MockupElement[] } = {}): MockupElement {
    return {
        id: args.id || ++idGen,
        type: args.type || "Col",
        children: args.children || []
    };
}

function createMockupInit(args: { type?: MockupElementType, id?: number, children?: MockupElement[] } = {}): MockupElement {
    return createMockupData({
        id: args.id || 1,
        type: args.type || "Row",
        children: args.children || []
    });
}

describe("mockupReducers", () => {
    it("returns initial state", () => {

        const state = mockupReducers(undefined, {} as any);

        expect(state).toBeDefined();
        expect(state.id).toBe(1);
        expect(state.type).toBe("Row");
        expect(state.children).toBeDefined();
    });

    describe("Add", () => {

        it("adds a new element to empty list.", () => {

            const state = mockupReducers(undefined, MockupActions.add("Col", 1, 0));

            expect(state.children).toHaveLength(1);
        });

        it("inserts a new element to position.", () => {
            const newElementAction = MockupActions.add("Col", 1, 1);

            const state = mockupReducers(
                createMockupInit({ children: [createMockupData(), createMockupData()] })
                , newElementAction);

            expect(state.children).toHaveLength(3);
            expect(state.children[1]).toBe(newElementAction.newElement);
        });

        it("inserts a new element to child position.", () => {
            const root = createMockupInit({ children: [createMockupData(), createMockupData()] })
            const newElementAction = MockupActions.add("Col", root.children[1].id, 0);

            const state = mockupReducers(
                root
                , newElementAction);

            expect(state.children).toHaveLength(2);
            expect(state.children[1].children).toHaveLength(1);
            expect(state.children[1].children[0]).toBe(newElementAction.newElement);
        });
    });

    describe("Remove", () => {
        it("removes existing element", () => {
            const root = createMockupInit({
                children:
                    [
                        createMockupData(),
                        createMockupData(),
                        createMockupData()
                    ]
            });

            const state = mockupReducers(root, MockupActions.remove(root.children[1].id));

            expect(state.children).not.toBe(root.children);
            expect(state.children).not.toContain(root.children[1]);
        });
        
        it("removes existing child element", () => {
            const root = createMockupInit({
                children:
                    [
                        createMockupData(),
                        createMockupData({
                            children: [
                                createMockupData(),
                                createMockupData(),
                                createMockupData()
                            ]
                        }),
                    ]
            });

            const state = mockupReducers(root, MockupActions.remove(root.children[1].children[1].id));

            expect(state.children).not.toBe(root.children);
            expect(state.children[1].children).not.toContain(root.children[1].children[1]);
        });
    });
});