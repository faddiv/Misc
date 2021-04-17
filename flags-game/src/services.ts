import { AlertProvider } from "./alerts";
import { injector } from "./react-injector";

export const appServiceCollection = {
  alert: () => new AlertProvider()
};

export const { withServices, useService, useServices, ServiceContextProvider } = injector(appServiceCollection);

