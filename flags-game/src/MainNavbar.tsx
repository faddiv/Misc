import { FunctionComponent } from "react";
import { Navbar, Nav } from 'react-bootstrap';
import { Link, NavLink } from "react-router-dom";

interface MainNavbarProps {

}

export const MainNavbar: FunctionComponent<MainNavbarProps> = () => {
  return (
    <Navbar bg="dark" variant="dark" expand="lg" sticky="top" as="header" collapseOnSelect>
      <Navbar.Brand as={Link} to="/">Flag Game</Navbar.Brand>
      <Navbar.Toggle aria-controls="main-navbar-nav" />
      <Navbar.Collapse id="main-navbar-nav">
        <Nav className="mr-auto">
          <Nav.Link as={NavLink} to="/" exact={true} eventKey="1">Game</Nav.Link>
          <Nav.Link as={NavLink} to="/playLists" eventKey="2">Playlists</Nav.Link>
        </Nav>
      </Navbar.Collapse>
    </Navbar>
  );
};
