import { FunctionComponent, StrictMode } from "react";
import "./index.scss";
import App from "./App";
import { Privacy } from "./Privacy";
import { render } from "react-dom";

var rootElement = document.getElementById("root");
if (rootElement) {
  const componentName = rootElement.dataset["reactComponent"];
  if (componentName) {
    const components: { [key: string]: FunctionComponent } = { App, Privacy };
    const Component = components[componentName];
    if (Component) {
      render(
        <StrictMode>
          <Component />
        </StrictMode>,
        rootElement
      );
    } else {
      console.error("Invalid component name: ", componentName, "Components: ", components);
    }
  }
}
