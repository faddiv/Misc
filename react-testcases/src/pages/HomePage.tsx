import { FunctionComponent } from "react";
import logo from '../logo.svg';

interface HomePageProps {

}

export const HomePage: FunctionComponent<HomePageProps> = () => {
  return (
    <>
      <img src={logo} className="app-logo" alt="logo" />
      <p>
        Edit <code>src/App.tsx</code> and save to reload.
    </p>
      <a
        className="App-link"
        href="https://reactjs.org"
        target="_blank"
        rel="noopener noreferrer"
      >
        Learn React
    </a>
    </>
  );
};
