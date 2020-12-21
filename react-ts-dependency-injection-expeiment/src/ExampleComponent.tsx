import { FunctionComponent } from "react";
import { injector } from "./react-injector/injectServices";

const { withServices, useService, useServices } = injector({
  service1: () => "Hello",
  service2: () => 42,
  service3: () => new String("from service"),
  service4: () => "from service",
  service5: () => {return { important: "Do remember" };}
});

interface ExampleComponentProps {
  service1: any,
  service3: any,
  otherProp: any
}

const ExampleComponent: FunctionComponent<ExampleComponentProps> = ({ service1, service3, otherProp }) => {
  const s2 = useService("service2");
  const service5 = useService("service5");
  const { service2, service4 } = useServices("service2", "service4")

  return (
    <div>{service1} {service3} {otherProp} {s2} {service2} {service5.important} {service4}</div>
  );
};

const NewComponent = withServices("service1", "service3")(ExampleComponent);

export const ExampleComponentUsage: FunctionComponent<any> = ({ ...rest }) => {
  return (
    <NewComponent otherProp=". other."></NewComponent>
  );
};
