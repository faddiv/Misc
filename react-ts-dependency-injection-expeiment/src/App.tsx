import React from 'react';
import logo from './logo.svg';
import './App.css';
import { ExampleComponentUsage } from './ExampleComponent';
import { appServiceCollection, ServiceContextProvider } from './appServices';


function App() {
  var modified = {
    ...appServiceCollection,
    service1: () => "xxx"
  }
  return (
    <ServiceContextProvider services={modified}>
      <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo" />
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
          <ExampleComponentUsage />
        </header>
      </div>
    </ServiceContextProvider>
  );
}

export default App;
