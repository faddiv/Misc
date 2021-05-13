import { ChangeEvent, FunctionComponent, useCallback, useMemo } from "react";
import { Form } from "react-bootstrap";
import { FlagInfo } from "../../flagsService/flagList";

interface CountryAutocompleteProps {
  flags: FlagInfo[],
  value: string;
  onValueChanged: (newValue: string) => void;
}

export const CountryAutocomplete: FunctionComponent<CountryAutocompleteProps> = ({ flags, value, onValueChanged }) => {
  const filteredFlagList = useMemo(() => {
    if (value === "")
      return [] as FlagInfo[];
    const regex1 = new RegExp("^" + value, "i");
    const regex2 = new RegExp(value, "i");
    const list = [
      ...flags.filter(e => e.Hun.search(regex1) !== -1),
      ...flags.filter(e => e.Hun.search(regex2) !== -1 && e.Hun.search(regex1) === -1)
    ];
    if (list.length >= 1 && list[0].Hun === value)
      return [] as FlagInfo[];
    return list.slice(0, 8);
  }, [value, flags]);
  const changeHandler = useCallback((evt: ChangeEvent<HTMLInputElement>) => {
    onValueChanged(evt.target.value);
  }, [onValueChanged]);
  return (
    <>
      <Form.Control type="text" name="guess" autoComplete="off" list="flag-names" value={value} onChange={changeHandler} />
      <datalist id="flag-names">
        {filteredFlagList.map(flag => <option key={flag.Pic} value={flag.Hun} />)}
      </datalist>
    </>
  );
};
