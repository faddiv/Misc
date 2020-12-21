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

const { withServices } = injector({
  service1: () => "Hello",
  service2: () => "Hello2",
  service3: () => new String("from service"),
  service4: () => "from service",
  service5: () => "from service"
});

const NewComponent = withServices("service1", "service3")(ExampleComponent);

export const ExampleComponentUsage: FunctionComponent<any> = ({ ...rest }) => {
  return (
    <NewComponent otherProp=". other."></NewComponent>
  );
};
