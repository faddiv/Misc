import { FunctionComponent } from "react";
import { Button, Card, Col, Form, Row } from "react-bootstrap";
import { useChangeToSetterHandler } from "../../common";
import { useFlagFilter } from "../../flagFilter";
import { usePlayList } from "../../flagsService/usePlayLists";
import { PlSelectElement } from "./PlSelectElement";
import { SelectableFlagList } from "./SelectableFlagList";

interface PlayListsPageProps {

}

export const PlayListsPage: FunctionComponent<PlayListsPageProps> = () => {
  const { plList, selected, flagCheckedHandler, changed, nameChanged, saveHandler, newElementHandler, selectElement } = usePlayList();
  const { searchText, filterFlags, filteredFlags } = useFlagFilter(selected?.flags || null, selected?.id || -1);
  const searchTextChangeHandler = useChangeToSetterHandler(filterFlags);
  return (
    <>
      <Row>
        <Col md={3}>
          <Row>
            {plList.map(value =>
              <Col key={value.id} xs={6} md={12}><PlSelectElement playList={value} selected={value.id === selected?.id} onPlayListSelected={selectElement} /></Col>
            )}
          </Row>
          <Button variant="Primary" className="w-100" onClick={newElementHandler}>Add new</Button>
        </Col>
        {selected &&
          <Col>
            <Card>
              <Card.Body>
                <Form inline onSubmit={saveHandler}>
                  <Form.Group>
                    <Form.Label>Name</Form.Label>
                    <Form.Control className="mx-2" value={selected.name} onChange={nameChanged} />
                  </Form.Group>
                  {selected.invalids &&

                    <Form.Group>
                      <Form.Label>Name</Form.Label>
                      <Form.Control plaintext className="mx-2" readOnly value={JSON.stringify(selected.invalids)} />
                    </Form.Group>
                  }
                  <Button type="submit" variant={changed ? "primary" : "secondary"} className="mx-2">Save</Button>
                  <Form.Group>
                    <Form.Label>Filter</Form.Label>
                    <Form.Control className="mx-2" value={searchText} onChange={searchTextChangeHandler} />
                  </Form.Group>
                </Form>
              </Card.Body>
            </Card>
            <SelectableFlagList flags={filteredFlags} onFlagChecked={flagCheckedHandler} />
          </Col>
        }
      </Row>
    </>
  );
};
