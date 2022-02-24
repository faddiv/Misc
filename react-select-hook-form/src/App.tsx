import "./App.css";
import { DefaultOption, fewData } from "./DefaultOption";
import { useForm } from "react-hook-form";
import Select from "./SelectExt/WrapperCreatable";
import { Button, Form, Stack } from "react-bootstrap";
import cl from "classnames";
import { useState } from "react";

interface MyModel {
  simpleSelect: DefaultOption | null;
  multiSelect: DefaultOption[] | null;
}

function App() {
  const [isMulti, setIsMulti] = useState(true);
  //const selectRef = useRef<SimpleSelect<MyModel, true>>();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
    setValue,
  } = useForm<MyModel>({
    mode: "onBlur",
  });

  const onSubmit = (data: MyModel) => {
    console.log("Single Select: ", data.simpleSelect);
    console.log("Multi Select: ", JSON.stringify(data.multiSelect));
    reset();
  };

  return (
    <div className="App">
      <Form noValidate onSubmit={handleSubmit(onSubmit)}>
        <Form.Group className="mb-3" controlId="simpleSelect">
          <Form.Label>Simple select</Form.Label>
          <Select
            {...register("simpleSelect", { required: true })}
            isClearable
            options={fewData}
            className={cl({ "is-invalid": !!errors.simpleSelect })}
          />
          {errors.simpleSelect ? <Form.Control.Feedback type="invalid">Simple Select required!</Form.Control.Feedback> : null}
        </Form.Group>

        <Form.Group className="mb-3" controlId="multiSelect">
          <Form.Label>Multi select</Form.Label>
          <Select
            {...register("multiSelect", { required: true })}
            isClearable
            isMulti={isMulti}
            options={fewData}
            className={cl({ "is-invalid": !!errors.multiSelect })}
          />
          {errors.simpleSelect ? <Form.Control.Feedback type="invalid">Multi Select required!</Form.Control.Feedback> : null}
        </Form.Group>

        <Stack direction="horizontal" gap={3} className="justify-content-center">
          <Button variant="primary" type="submit">
            Submit
          </Button>
          <Button variant="secondary" type="button" onClick={() => setValue("multiSelect", [fewData[0]])}>
            Set First
          </Button>
          <Button
            variant="secondary"
            type="button"
            onClick={() => {
              setIsMulti((multi) => !multi);
            }}
          >
            Set Second
          </Button>
          <Button variant="secondary" type="button" onClick={() => setValue("multiSelect", [fewData[2], fewData[3]])}>
            Set Third
          </Button>
        </Stack>
      </Form>
    </div>
  );
}

export default App;
