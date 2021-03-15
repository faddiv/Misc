import { Container, Navbar, Nav, NavDropdown, Form, FormControl, Button } from 'react-bootstrap';
import { BrowserRouter } from "react-router-dom";
import { Switch, Route } from "react-router";
import { HomePage } from './pages/HomePage';
import { SomePage } from './pages/SomePage';
import { MainNavbar } from './MainNavbar';

function App() {
  return (
    <BrowserRouter>
      <MainNavbar />
      <Container className="main" as="main" role="main">
        <Switch>
          <Route path="/" exact={true} component={HomePage} />
          <Route path="/somePage" component={SomePage} />
        </Switch>
      </Container>
    </BrowserRouter>
  );
}

export default App;
