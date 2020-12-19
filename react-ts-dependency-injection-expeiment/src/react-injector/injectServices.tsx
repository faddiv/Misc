import { ComponentType, PropsWithChildren } from "react";

type ServiceCreator = () => any;

interface ServiceCollection {
  [key: string]: ServiceCreator;
}

export function injector<SC extends ServiceCollection>(services: SC) {
  const instances: any = {};
  function componentInjector<P, T extends P | SC, S extends Extract<keyof T, string>>(Component: ComponentType<P>, ...injectable: S[]): ComponentType<Omit<P, S>> {
    function ComponentWithServices(props: PropsWithChildren<Omit<P, S>>) {
      const newProps: any = { ...props };
      for (let index = 0; index < injectable.length; index++) {
        const key = injectable[index];
        if (typeof instances[key] === "undefined") {
          const serviceCreator = services[key];
          if (typeof serviceCreator !== "function") {
            throw new Error(`the service '${key}' must be a function`);
          }
          instances[key] = serviceCreator();
        }
        newProps[key] = instances[key];
      }
      return <Component {...newProps as unknown as P} />;
    }
    return ComponentWithServices;
  }
  return {
    componentInjector
  }
}
