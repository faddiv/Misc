import { FunctionComponent } from "react";
import { injector, injectServices } from "./react-injector/injectServices";

interface ExampleComponentProps {
  service1: any,
  service2: any,
  otherProp: any
}

const ExampleComponent: FunctionComponent<ExampleComponentProps> = ({ service1, service2, otherProp, ...rest }) => {
  return (
    <div>OK</div>
  );
};

const inject = injector({
  service1: {},
  service3: {}
});

const NewComponent = inject(ExampleComponent, "service1");

const ExampleComponentUsage: FunctionComponent<any> = ({ ...rest }) => {
  return (
    <NewComponent otherProp="" service2=""></NewComponent>
  );
};
