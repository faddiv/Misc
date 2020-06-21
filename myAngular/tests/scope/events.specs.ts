import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import * as _ from 'lodash';
"use strict";

describe("scope", () => {
    describe("events", () => {

        var parent: IScopeEx;
        var scope: IScopeEx;
        var child: IScopeEx;
        var isolatedChild: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            parent = injector.get("$rootScope");
            scope = parent.$new();
            child = scope.$new();
            isolatedChild = scope.$new(true);
        });

        it("allows registering listeners", () => {
            var listener1 = jasmine.createSpy("listener1");
            var listener2 = jasmine.createSpy("listener2");
            var listener3 = jasmine.createSpy("listener3");

            scope.$on("someEvent", listener1);
            scope.$on("someEvent", listener2);
            scope.$on("someOtherEvent", listener3);

            //Non refactored, private element.
            /*expect(scope.$$listeners).toEqual({
                someEvent: [listener1, listener2],
                someOtherEvent: [listener3]
              });*/
            //Refactored, only non private elements.
            scope.$emit("someEvent");
            scope.$emit("someOtherEvent");
            expect(listener1).toHaveBeenCalled();
            expect(listener2).toHaveBeenCalled();
            expect(listener3).toHaveBeenCalled();
        });

        it("registers different listeners for every scope", () => {
            /*var listener1 = () => { };
            var listener2 = () => { };
            var listener3 = () => { };

            scope.$on("someEvent", listener1);
            child.$on("someEvent", listener2);
            isolatedChild.$on("someEvent", listener3);

            //Non refactored, private elements.
            expect(scope.$$listeners).toEqual({
                someEvent: [listener1]
            });
            expect(child.$$listeners).toEqual({
                someEvent: [listener2]
            });
            expect(isolatedChild.$$listeners).toEqual({
                someEvent: [listener3]
            });*/
        });

        _.forEach(["$emit", "$broadcast"], method => {
            it("calls the listeners of the matching event on " + method, () => {
                var listener1 = jasmine.createSpy("listener1");
                var listener2 = jasmine.createSpy("listener2");

                scope.$on("someEvent", listener1);
                scope.$on("someOtherEvent", listener2);

                scope[method]("someEvent");

                expect(listener1).toHaveBeenCalled();
                expect(listener2).not.toHaveBeenCalled();
            });

            it("passes an event object with a name to listeners on " + method, () => {
                var listener = jasmine.createSpy("listener1");

                scope.$on("someEvent", listener);

                scope[method]("someEvent");

                expect(listener).toHaveBeenCalled();
                expect(listener.calls.mostRecent().args[0].name).toEqual("someEvent");
            });

            it("passes the same event object to each listener on " + method, () => {
                var listener1 = jasmine.createSpy("listener1");
                var listener2 = jasmine.createSpy("listener2");

                scope.$on("someEvent", listener1);
                scope.$on("someEvent", listener2);

                scope[method]("someEvent");

                var event1 = listener1.calls.mostRecent().args[0];
                var event2 = listener1.calls.mostRecent().args[0];
                expect(event1).toBe(event2);
            });

            it("passes additional arguments to listeners on " + method, () => {
                var listener = jasmine.createSpy("listener1");

                scope.$on("someEvent", listener);

                scope[method]("someEvent", "and", ["additional", "arguments"], "...");

                expect(listener.calls.mostRecent().args[1]).toEqual("and");
                expect(listener.calls.mostRecent().args[2]).toEqual(["additional", "arguments"]);
                expect(listener.calls.mostRecent().args[3]).toEqual("...");
            });

            it("returns the event object on " + method, () => {
                var returnedEvent: IAngularEvent = scope[method]("someEvent");

                expect(returnedEvent).toBeDefined();
                expect(returnedEvent.name).toEqual("someEvent");
            });

            it("can be deregistered " + method, () => {
                var listener = jasmine.createSpy("listener1");
                var deregister = scope.$on("someEvent", listener);

                deregister();

                scope[method]("someEvent");

                expect(listener).not.toHaveBeenCalled();
            });

            it("does not skip the next listener when removed on " + method, () => {
                var deregister: () => void;
                var listener = () => {
                    deregister();
                };
                var nextListener = jasmine.createSpy("nextListener");

                deregister = scope.$on("someEvent", listener);
                scope.$on("someEvent", nextListener);

                scope[method]("someEvent");

                expect(nextListener).toHaveBeenCalled();
            });
        });

        it("propagates up the scope hierarchy on $emit", () => {
            var parentListener = jasmine.createSpy("parentListener");
            var scopeListener = jasmine.createSpy("scopeListener");
            var childListener = jasmine.createSpy("childListener");
            var isolatedChildListener = jasmine.createSpy("childListener");

            parent.$on("someEvent", parentListener);
            scope.$on("someEvent", scopeListener);
            child.$on("someEvent", childListener);
            isolatedChild.$on("someEvent", isolatedChildListener);

            scope.$emit("someEvent");

            expect(scopeListener).toHaveBeenCalled();
            expect(childListener).not.toHaveBeenCalled();
            expect(isolatedChildListener).not.toHaveBeenCalled();
            expect(parentListener).toHaveBeenCalled();
        });

        it("propagates the same event up on $emit", () => {
            var parentListener = jasmine.createSpy("parentListener");
            var scopeListener = jasmine.createSpy("scopeListener");

            parent.$on("someEvent", parentListener);
            scope.$on("someEvent", scopeListener);

            scope.$emit("someEvent");

            var scopeEvent = scopeListener.calls.mostRecent().args[0];
            var parentEvent = parentListener.calls.mostRecent().args[0];
            expect(scopeEvent).toBe(parentEvent);
        });

        it("propagates down the scope hierarchy on $broadcast", () => {
            var parentListener = jasmine.createSpy("parentListener");
            var scopeListener = jasmine.createSpy("scopeListener");
            var childListener = jasmine.createSpy("childListener");
            var isolatedChildListener = jasmine.createSpy("childListener");

            parent.$on("someEvent", parentListener);
            scope.$on("someEvent", scopeListener);
            child.$on("someEvent", childListener);
            isolatedChild.$on("someEvent", isolatedChildListener);

            scope.$broadcast("someEvent");

            expect(scopeListener).toHaveBeenCalled();
            expect(parentListener).not.toHaveBeenCalled();
            expect(childListener).toHaveBeenCalled();
            expect(isolatedChildListener).toHaveBeenCalled();
        });

        it("propagates the same event down on $broadcast", () => {
            var scopeListener = jasmine.createSpy("scopeListener");
            var childListener = jasmine.createSpy("childListener");
            var isolatedChildListener = jasmine.createSpy("childListener");

            scope.$on("someEvent", scopeListener);
            child.$on("someEvent", childListener);
            isolatedChild.$on("someEvent", isolatedChildListener);

            scope.$broadcast("someEvent");

            var scopeEvent = scopeListener.calls.mostRecent().args[0];
            var childEvent = childListener.calls.mostRecent().args[0];
            var isolatedchildEvent = isolatedChildListener.calls.mostRecent().args[0];
            expect(scopeEvent).toBe(childEvent);
            expect(scopeEvent).toBe(isolatedchildEvent);
        });

        it("attaches targetScope on $emit", () => {
            var parentListener = jasmine.createSpy("parentListener");
            var scopeListener = jasmine.createSpy("scopeListener");

            parent.$on("someEvent", parentListener);
            scope.$on("someEvent", scopeListener);

            scope.$emit("someEvent");

            expect(scopeListener.calls.mostRecent().args[0].targetScope).toBe(scope);
            expect(parentListener.calls.mostRecent().args[0].targetScope).toBe(scope);
        });

        it("attaches targetScope on $broadcast", () => {
            var childListener = jasmine.createSpy("parentListener");
            var scopeListener = jasmine.createSpy("scopeListener");

            child.$on("someEvent", childListener);
            scope.$on("someEvent", scopeListener);

            scope.$broadcast("someEvent");

            expect(scopeListener.calls.mostRecent().args[0].targetScope).toBe(scope);
            expect(childListener.calls.mostRecent().args[0].targetScope).toBe(scope);
        });

        it("attaches currentScope on $emit", () => {
            var currentScopeOnScope, currentScopeOnParent;

            var scopeListener = (event: IAngularEvent) => {
                currentScopeOnScope = event.currentScope;
            };

            var parentListener = (event: IAngularEvent) => {
                currentScopeOnParent = event.currentScope;
            };

            parent.$on("someEvent", parentListener);
            scope.$on("someEvent", scopeListener);

            scope.$emit("someEvent");

            expect(currentScopeOnScope).toBe(scope);
            expect(currentScopeOnParent).toBe(parent);
        });

        it("attaches currentScope on $broadcast", () => {
            var currentScopeOnScope, currentScopeOnChild;

            var scopeListener = (event: IAngularEvent) => {
                currentScopeOnScope = event.currentScope;
            };

            var childListener = (event: IAngularEvent) => {
                currentScopeOnChild = event.currentScope;
            };

            child.$on("someEvent", childListener);
            scope.$on("someEvent", scopeListener);

            scope.$broadcast("someEvent");

            expect(currentScopeOnScope).toBe(scope);
            expect(currentScopeOnChild).toBe(child);
        });

        _.forEach(["$emit", "$broadcast"], method => {
            it("sets currentScope to null after propagation on " + method, () => {
                var event: IAngularEvent;

                var scopeListener = (evt: IAngularEvent) => {
                    event = evt;
                };

                scope.$on("someEvent", scopeListener);

                scope[method]("someEvent");

                expect(event.currentScope).toBe(null);
            });
        });

        it("does not propagate to parents when stopped", () => {
            var scopeListener = (evt: IAngularEvent) => {
                evt.stopPropagation();
            };
            var parentListener = jasmine.createSpy("parentListener");

            scope.$on("someEvent", scopeListener);
            parent.$on("someEvent", parentListener);

            scope.$emit("someEvent");

            expect(parentListener).not.toHaveBeenCalled();
        });
        
        it("is received by listeners on current scope after being stopped", () => {
            var listener1 = (evt: IAngularEvent) => {
                evt.stopPropagation();
            };
            var listener2 = jasmine.createSpy("listener2");

            scope.$on("someEvent", listener1);
            scope.$on("someEvent", listener2);

            scope.$emit("someEvent");

            expect(listener2).toHaveBeenCalled();
        });
        
        _.forEach(["$emit", "$broadcast"], method => {
            it("is sets defaultPrevented when preventDefault called on " + method, () => {

                var listener = (evt: IAngularEvent) => {
                    evt.preventDefault();
                };

                scope.$on("someEvent", listener);

                var event = scope[method]("someEvent");

                expect(event.defaultPrevented).toBe(true);
            });
        });

        it("fires $destroy when destroyed", () => {
            var listener = jasmine.createSpy("listener");
            scope.$on("$destroy", listener);

            scope.$destroy();

            expect(listener).toHaveBeenCalled();
        });

        it("fires $destroy on children destroyed", () => {
            var listener = jasmine.createSpy("listener");

            child.$on("$destroy", listener);

            scope.$destroy();

            expect(listener).toHaveBeenCalled();
        });

        it("no longers calls listeners after destroyed", () => {
            var listener = jasmine.createSpy("listener");
            scope.$on("someEvent", listener);

            scope.$destroy();

            scope.$emit("someEvent");

            expect(listener).not.toHaveBeenCalled();
        });
        
        _.forEach(["$emit", "$broadcast"], method => {
            it("does not stop on exceptions on " + method, () => {

                var listener1 = (evt: IAngularEvent) => {
                    throw "listener1 throwing an exception";
                };
                var listener2 = jasmine.createSpy("listener2");
                scope.$on("someEvent", listener1);
                scope.$on("someEvent", listener2);

                scope[method]("someEvent");

                expect(listener2).toHaveBeenCalled();
            });
        });

    });
});