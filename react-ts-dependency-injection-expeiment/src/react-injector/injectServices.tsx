import { Component, ComponentType, FunctionComponent, PropsWithChildren } from "react";
import { server } from "typescript";

export function injectServices<P, S extends Extract<keyof P, string>>(Component: FunctionComponent<P>, ...services: S[]): FunctionComponent<Omit<P, S>> {
  function ComponentWithServices(props: PropsWithChildren<Omit<P, S>>) {
    return <Component {...props as unknown as P} />;
  }
  return ComponentWithServices;
}

export function injector<SC>(services: SC) {
  return function injectServices<P, T extends P | SC, S extends Extract<keyof T, string>>(Component: FunctionComponent<P>, ...services: S[]): FunctionComponent<Omit<P, S>> {
    function ComponentWithServices(props: PropsWithChildren<Omit<P, S>>) {
      return <Component {...props as unknown as P} />;
    }
    return ComponentWithServices;
  }
}
