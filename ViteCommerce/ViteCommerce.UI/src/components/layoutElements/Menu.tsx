import { FunctionComponent } from "react";
import { Navbar, Container, Nav, NavDropdown } from "react-bootstrap";
import { UserMenuItem } from "./UserMenuItem";

export const Menu: FunctionComponent = () => {
  return (
    <Navbar
      bg="light"
      expand="lg"
      className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3"
    >
      <Container>
        <Navbar.Brand href="/">React-Bootstrap</Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="ms-auto">
            <Nav.Link href="/">Home</Nav.Link>
            <Nav.Link href="/counter">Counter</Nav.Link>
            <Nav.Link href="/fetch-data">Fetch Data</Nav.Link>
            <UserMenuItem />
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};
