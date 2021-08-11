import { useMemo } from "react";
import { FunctionComponent, useState } from "react";
import { Form } from "react-bootstrap";
import Select, { ActionMeta } from "react-select";
import Creatable from "react-select/creatable";

interface HomePageProps {

}
type OptionType = { label: string; value: string };

function getValue(item: OptionType) {
  return item.value;
}

function getLabel(item: OptionType) {
  return item.label;
}

const options: OptionType[] = [
  { value: 'chocolate', label: 'Chocolate' },
  { value: 'strawberry', label: 'Strawberry' },
  { value: 'vanilla', label: 'Vanilla' }
]

export const HomePage: FunctionComponent<HomePageProps> = () => {
  const [value, setValue] = useState<any>(null);
  const [actionMeta, setActionMeta] = useState<any>(null);

  const onChange = (newValue: OptionType, meta: ActionMeta<any>) => {
    if (newValue === null) {
      setValue(newValue);
    } else {
      setValue(newValue.value);
    }
    setActionMeta(newValue);
  }

  const sel = useMemo(() => {
    if (value === null)
      return value;
    var option = options.find(e => e.value === value);
    if (option)
      return option;
    return {
      value: value,
      label: value,
      __isNew__: true
    }
  }, [value]);
  return (

    <Form>
      <h1>Header 1</h1>
      <Form.Row>
        <Form.Label>Select1</Form.Label>
        <Creatable options={options}
          isClearable
          value={sel}
          onChange={onChange}
          className="w-100" />
      </Form.Row>
      <Form.Row>
        <Form.Label>Select2</Form.Label>
        <Creatable options={options}
          isClearable
          value={sel}
          onChange={onChange}
          className="w-100" />
      </Form.Row>
      <div>
        Value: {JSON.stringify(value)}
      </div>
      <div>
        Meta: {JSON.stringify(actionMeta)}
      </div>
    </Form>
  );
};
