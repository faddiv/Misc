import { FunctionComponent } from "react";
import { Navbar, Nav } from 'react-bootstrap';
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
          <Nav.Link as={NavLink} to="/example1">Event With Child</Nav.Link>
          <Nav.Link as={NavLink} to="/example2">Context Drilling</Nav.Link>
        </Nav>
      </Navbar.Collapse>
    </Navbar>
  );
};
