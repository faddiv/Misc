import { Container } from 'react-bootstrap';
import { BrowserRouter } from "react-router-dom";
import { Switch, Route } from "react-router";
import { HomePage } from './pages/HomePage';
import { MainNavbar } from './MainNavbar';
import { EventWithChild } from './pages/EventWithChild/EventWithChild';
import { ContextDrilling } from './pages/ContextDrilling/ContextDrilling';

function App() {
  return (
    <BrowserRouter>
      <MainNavbar />
      <Container className="main" as="main" role="main">
        <Switch>
          <Route path="/" exact={true} component={HomePage} />
          <Route path="/example1" component={EventWithChild} />
          <Route path="/example2" component={ContextDrilling} />
          
        </Switch>
      </Container>
    </BrowserRouter>
  );
}

export default App;
