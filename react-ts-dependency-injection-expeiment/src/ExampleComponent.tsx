import { FunctionComponent } from "react";
import { injector } from "./react-injector/injectServices";

interface ExampleComponentProps {
  service1: any,
  service3: any,
  otherProp: any
}

const ExampleComponent: FunctionComponent<ExampleComponentProps> = ({ service1, service3, otherProp }) => {
  return (
    <div>{service1} {service3} {otherProp}</div>
  );
};

const { componentInjector: componentInjector } = injector({
  service1: () => "Hello",
  service3: () => new String("from service")
});

const NewComponent = componentInjector(ExampleComponent, "service1", "service3");

export const ExampleComponentUsage: FunctionComponent<any> = ({ ...rest }) => {
  return (
    <NewComponent otherProp=". other."></NewComponent>
  );
};
