import { ComponentType, PropsWithChildren } from "react";

type ServiceCreator = () => any;

interface ServiceCollection {
  [key: string]: ServiceCreator;
}

export function injector<SC extends ServiceCollection>(services: SC) {
  const instances: any = {};

  function getService(injectable: string): any {
    if (typeof instances[injectable] === "undefined") {
      const serviceCreator = services[injectable];
      if (typeof serviceCreator !== "function") {
        throw new Error(`the service '${injectable}' must be a function`);
      }
      instances[injectable] = serviceCreator();
    }
    return instances[injectable];
  }

  function useService<S extends Extract<keyof SC, string>, O extends ReturnType<SC[S]>>(injectable: S): O {
    return getService(injectable);
  }

  function useServices<S extends Extract<keyof SC, string>, O extends ReturnType<SC[S]>>(...injectable: S[]): { [key in S]: O } {
    const newProps: any = {};
    for (let index = 0; index < injectable.length; index++) {
      const key = injectable[index];
      newProps[key] = getService(key);
    }
    return newProps;
  }

  function withServices<S extends Extract<keyof SC, string>>(...injectable: S[]) {

    return function componentInjector<P extends { [key in S]: any }>(Component: ComponentType<P>): ComponentType<Omit<P, S>> {

      function ComponentWithServices(props: PropsWithChildren<Omit<P, S>>) {
        const newProps: any = {
          ...props,
          ...useServices(...injectable)
        };
        return <Component {...newProps as P} />;
      }

      return ComponentWithServices;
    }
  }

  return {
    withServices,
    useServices,
    useService
  }
}
