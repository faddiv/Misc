import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    describe("inheritance", () => {
        var parent: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            parent = injector.get("$rootScope");
        });

        it("inherits the parent's properties", () => {
            parent.text = "asdf";

            var child: IScopeEx = parent.$new();

            expect(child.text).toEqual("asdf");
        });

        it("does not cause a parent to inherit its properties", () => {
            var child: IScopeEx = parent.$new();
            child.text = "asdf";
            expect(parent.text).toBeUndefined();
        });

        it("inherits the parents properties whenever they are defined", () => {
            var child: IScopeEx = parent.$new();

            parent.text = "asdf";
            expect(child.text).toEqual("asdf");
        });

        it("can manipulate a parent scopes property", () => {
            var child: IScopeEx = parent.$new();

            parent.array = [1, 2];
            child.array.push(3);
            expect(child.array).toEqual([1, 2, 3]);
        });

        it("can watch a property in the parent", () => {
            var child: IScopeEx = parent.$new();
            parent.text = "asdf";
            child.counter = 0;

            child.$watch(
                (scope: IScopeEx) => scope.text,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                });

            child.$digest();
            expect(child.counter).toBe(1);

            parent.text = "fdsa";
            child.$digest();
            expect(child.counter).toBe(2);
        });

        it("can be nested at any depth", () => {
            var a: IScopeEx = parent.$new();
            var aa: IScopeEx = a.$new();
            var bb: IScopeEx = aa.$new();

            parent.number = 1;

            expect(a.number).toBe(1);
            expect(aa.number).toBe(1);
            expect(bb.number).toBe(1);

            aa.number2 = 2;

            expect(bb.number2).toBe(2);
            expect(a.number2).toBeUndefined();
            expect(parent.number2).toBeUndefined();
        });

        it("shadows a parents property with the same name", () => {

            var child: IScopeEx = parent.$new();

            parent.text = "parent";
            child.text = "child";

            expect(parent.text).toEqual("parent");
            expect(child.text).toEqual("child");
        });

        it("does not shadow members of parent scopes attributes", () => {

            var child: IScopeEx = parent.$new();

            parent.array = [1];
            child.array.push(2);

            expect(parent.array).toEqual([1, 2]);
            expect(child.array).toEqual([1, 2]);
        });

        it("does not digest its parent(s)", () => {

            var child: IScopeEx = parent.$new();

            parent.name = "asdf";

            parent.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.text = newValue;
                });

            child.$digest();
            expect(child.text).toBeUndefined();
        });

        it("digest its children", () => {
            var child: IScopeEx = parent.$new();

            parent.name = "abc";

            child.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.text = newValue;
                });

            parent.$digest();
            expect(child.text).toBe("abc");
        });

        it("digests from root on $apply", () => {
            var child: IScopeEx = parent.$new();
            var child2: IScopeEx = child.$new();

            parent.name = "abc";
            parent.counter = 0;

            parent.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                });

            child2.$apply(scope => { });
            expect(parent.counter).toBe(1);
        });

        it("schedules a digest from root on $evalAsync", (done) => {
            var child: IScopeEx = parent.$new();
            var child2: IScopeEx = child.$new();

            parent.name = "abc";
            parent.counter = 0;

            parent.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                });

            child2.$evalAsync(scope => { });
            setTimeout(function () {
                expect(parent.counter).toBe(1);
                done();
            }, 50);
        });

        it("does not have access to parent attributes when isolated", () => {
            var child: IScopeEx = parent.$new(true);

            parent.text = "asdf";
            expect(child.text).toBeUndefined();
        });

        it("cannot watch parent attributes when isolated", () => {
            var child: IScopeEx = parent.$new(true);
            parent.name = "asdf";

            child.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.text = newValue;
                });

            child.$digest();
            expect(child.text).toBeUndefined();
        });

        it("digests its isolated children", () => {
            var child: IScopeEx = parent.$new(true);
            child.name = "asdf";

            child.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.text = newValue;
                });

            parent.$digest();
            expect(child.text).toBe("asdf");
        });

        it("digests from root on $apply when isolated", () => {
            var child: IScopeEx = parent.$new(true);
            var child2: IScopeEx = child.$new(true);

            parent.name = "abc";
            parent.counter = 0;

            parent.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                });

            child2.$apply(scope => { });

            expect(parent.counter).toBe(1);
        });

        it("schedules a digest from root on $evalAsync when isolated", (done) => {
            var child: IScopeEx = parent.$new(true);
            var child2: IScopeEx = child.$new(true);

            parent.name = "abc";
            parent.counter = 0;

            parent.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                });

            child2.$evalAsync(scope => { });

            child2.$evalAsync(scope => { });
            setTimeout(function () {
                expect(parent.counter).toBe(1);
                done();
            }, 50);
        });

        it("executes $evalAsync functions on isolated scopes", (done) => {
            var child: IScopeEx = parent.$new(true);

            child.$evalAsync((scope: IScopeEx) => {
                scope.asyncApplied = true;
            });

            setTimeout(function () {
                expect(child.asyncApplied).toBe(true);
                done();
            }, 50);
        });

        it("executes $$postDigest functions on isolated scopes", () => {
            var child: IScopeEx = parent.$new(true);

            child.$$postDigest(() => {
                child.postDigestDone = true;
            });

            child.$digest();
            expect(child.postDigestDone).toBe(true);
        });

        it("executes $applyAsync functions on isolated scopes", () => {
            var child: IScopeEx = parent.$new(true);
            var applied = false;

            parent.$applyAsync(() => {
                applied = true;
            });

            child.$digest();
            expect(applied).toBe(true);
        });

        it("can take some other scope as the parent", () => {
            var prototypeParent: IScopeEx = parent.$new();
            var hierarchyParent: IScopeEx = parent.$new();
            var child: IScopeEx = prototypeParent.$new(false, hierarchyParent);

            prototypeParent.number = 42;
            expect(child.number).toBe(42);

            child.counter = 0;
            child.$watch((scope: IScopeEx) => {
                scope.counter++;
                return 44;
            });

            prototypeParent.$digest();
            expect(child.counter).toBe(0);

            hierarchyParent.$digest();
            expect(child.counter).toBe(2);
        });

        it("is no longer digested when $destroy has been called", () => {
            var child: IScopeEx = parent.$new();

            child.number = 42;
            child.counter = 0;

            child.$watch((scope: IScopeEx) => {
                return scope.number;
            }, (newValue, oldValue, scope: IScopeEx) => {
                scope.counter++;
            });

            parent.$digest();
            expect(child.counter).toBe(1);

            child.number = 44;
            parent.$digest();
            expect(child.counter).toBe(2);

            child.number = 11;
            child.$destroy();
            parent.$digest();
            expect(child.counter).toBe(2);

            child.number = 115;
            child.$digest();
            expect(child.counter).toBe(2);
        });
    });
});