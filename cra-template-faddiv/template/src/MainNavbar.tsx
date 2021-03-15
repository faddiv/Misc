import { FunctionComponent } from "react";
import { Navbar, Nav, NavDropdown, Form, FormControl, Button } from 'react-bootstrap';
import { Link, NavLink } from "react-router-dom";

interface MainNavbarProps {

}

export const MainNavbar: FunctionComponent<MainNavbarProps> = () => {
  return (
    <Navbar bg="dark" variant="dark" expand="lg" sticky="top" as="header">
      <Navbar.Brand as={Link} to="/">Navbar</Navbar.Brand>
      <Navbar.Toggle aria-controls="main-navbar-nav" />
      <Navbar.Collapse id="main-navbar-nav">
        <Nav className="mr-auto">
          <Nav.Link as={NavLink} to="/" exact={true}>Home</Nav.Link>
          <Nav.Link as={NavLink} to="/somePage">Link</Nav.Link>
          <NavDropdown title="Dropdown" id="main-nav-dropdown">
            <NavDropdown.Item href="#action/3.1">Action</NavDropdown.Item>
            <NavDropdown.Item href="#action/3.2">Another action</NavDropdown.Item>
            <NavDropdown.Item href="#action/3.3">Something</NavDropdown.Item>
            <NavDropdown.Divider />
            <NavDropdown.Item href="#action/3.4">Separated link</NavDropdown.Item>
          </NavDropdown>
        </Nav>
        <Form inline>
          <FormControl type="text" placeholder="Search" className="mr-sm-2" />
          <Button variant="outline-info">Search</Button>
        </Form>
      </Navbar.Collapse>
    </Navbar>
  );
};
