import React from 'react';
import { Container, Row, Col, Button, Card } from 'reactstrap';
import './App.scss';
import { DraggableCol } from './DraggableElements/DraggableCol';
import { RecycleBin } from './RecycleBin';
import { TopNavbar } from './TopNavbar';
import { DraggableElementRenderer } from './DraggableElements/DraggableElementRenderer';
import { useSelector } from 'react-redux';
import { RootState } from './store/RootStateAndReducer';
import { DraggableRow } from './DraggableElements/DraggableRow';

const App: React.FunctionComponent = () => {
  var container = useSelector((state: RootState) => state.container);
  return (
    <Container fluid>
      <TopNavbar />
      <Row>
        <Col sm="1" className="toolbar">
          <Card><Button color="primary">Button</Button></Card>
          <Card><DraggableCol mockupElement={{ id: 0, type: "Col", children: [] }}></DraggableCol></Card>
          <Card><DraggableRow mockupElement={{ id: 0, type: "Row", children: [] }}></DraggableRow></Card>
          <Card><RecycleBin></RecycleBin></Card>
        </Col>
        <Col sm="11">
          <DraggableElementRenderer mockupElement={container} />
        </Col>
      </Row>
    </Container>
  );
}

export default App;
