import { Container } from 'react-bootstrap';
import { BrowserRouter } from "react-router-dom";
import { Switch, Route } from "react-router";
import { HomePage } from './pages/Home/HomePage';
import { PlayListsPage } from './pages/Playlists/PlayListsPage';
import { MainNavbar } from './MainNavbar';
import { ServiceContextProvider, appServiceCollection } from "./services";
import { AlertBox } from './alerts';

function App() {
  return (
    <BrowserRouter>
      <ServiceContextProvider services={appServiceCollection}>
        <MainNavbar />
        <Container className="main" as="main" role="main" fluid>
          <AlertBox />
          <Switch>
            <Route path="/" exact={true} component={HomePage} />
            <Route path="/playLists" component={PlayListsPage} />
          </Switch>
        </Container>
      </ServiceContextProvider>
    </BrowserRouter>
  );
}

export default App;
