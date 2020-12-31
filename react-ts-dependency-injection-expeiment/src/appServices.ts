import { injector } from "./react-injector";

export const appServiceCollection = {
  service1: () => "Hello",
  service2: () => 42,
  service3: () => { return { prop: "from service" }; },
  service4: () => "from service",
  service5: () => { return { important: "Do remember" }; }
};

export const { withServices, useService, useServices, ServiceProvider } = injector(appServiceCollection);
