import { ComponentType, createContext, FunctionComponent, PropsWithChildren, useContext } from "react";

export type ServiceCreator = () => any;

export interface ServiceCollection {
  [key: string]: ServiceCreator;
}

export interface ServiceProviderProps<SC extends ServiceCollection> {
  services?: SC;
}

/**
 * Creates the dependency injection container. Use like this:
 *
 * ```ts
 * export const { withServices, useService, useServices, ServiceProvider } = injector(services);
 * ```
 * @param services Collection of the service creators.
 */
export function injector<SC extends ServiceCollection>(services: SC) {
  const instances: any = {};
  const serviceConstructors = services;

  const context = createContext(services);
  const Provider = context.Provider;

  function getService(sc: SC, injectable: string): any {
    if (typeof instances[injectable] === "undefined") {
      const serviceCreator = sc[injectable];
      if (typeof serviceCreator !== "function") {
        throw new Error(`the service '${injectable}' must be a function`);
      }
      instances[injectable] = serviceCreator();
    }
    return instances[injectable];
  }

  function useService<S extends Extract<keyof SC, string>>(injectable: S): ReturnType<SC[S]> {
    const sc = useContext(context);
    return getService(sc, injectable);
  }

  function useServices<S extends Extract<keyof SC, string>>(...injectable: S[]): { [key in S]: ReturnType<SC[key]> } {
    const newProps: any = {};
    const sc = useContext(context);
    for (let index = 0; index < injectable.length; index++) {
      const key = injectable[index];
      newProps[key] = getService(sc, key);
    }
    return newProps;
  }

  function withServices<S extends Extract<keyof SC, string>>(...injectable: S[]) {

    return function componentInjector<P extends { [key in S]: ReturnType<SC[key]> }>(Component: ComponentType<P>): ComponentType<Omit<P, S>> {

      function ComponentWithServices(props: PropsWithChildren<Omit<P, S>>) {
        const newProps: any = {
          ...props,
          ...useServices.apply(undefined, injectable)
        };
        return <Component {...newProps as P} />;
      }

      return ComponentWithServices;
    }
  }

  const ServiceContextProvider: FunctionComponent<ServiceProviderProps<SC>> = ({ services, children }) => {

    return (
      <Provider value={services || serviceConstructors}>
        {children}
      </Provider>
    );
  }

  return {
    /**
     * Higher order component that injects the specified services into the react component, and excludes it from the typescript typing.
     * @param injectable Service names to be injected.
     */
    withServices,
    /**
     * Returns with multpile services specified by name.
     * @param injectable Service names to be injected.
     */
    useServices,
    /**
     * Returns with a single service specified by name.
     * @param injectable Service name to be injected.
     */
    useService,
    /**
     * Provides context to the service injection. If value provided, all the services will be replaced.
     */
    ServiceContextProvider
  }
}
