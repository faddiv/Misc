import { Container } from "react-bootstrap";
import { BrowserRouter } from "react-router-dom";
import { Route, Routes } from "react-router";
import { HomePage } from "./pages/HomePage";
import { SomePage } from "./pages/SomePage";
import { MainNavbar } from "./MainNavbar";

function App() {
  return (
    <BrowserRouter>
      <MainNavbar />
      <Container className="main" as="main" role="main">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/somePage" element={<SomePage />} />
        </Routes>
      </Container>
    </BrowserRouter>
  );
}

export default App;
