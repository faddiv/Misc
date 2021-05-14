import { ChangeEvent, FunctionComponent, useCallback, useMemo, useRef } from "react";
import { Form } from "react-bootstrap";
import { Typeahead } from "react-bootstrap-typeahead";
import { FlagInfo } from "../../flagsService/flagList";

interface CountryAutocompleteProps {
  flags: FlagInfo[],
  value: string;
  onValueChanged: (newValue: string) => void;
}
const mobile = /Mobile|Android|iP(hone|od)|IEMobile|BlackBerry|Kindle|Silk-Accelerated|(hpw|web)OS|Opera M(obi|ini)/.test(navigator.userAgent);

export const CountryAutocomplete: FunctionComponent<CountryAutocompleteProps> = ({ flags, value, onValueChanged }) => {
  const ref = useRef<Typeahead<FlagInfo>>(null);
  const changeHandler = useCallback((selections: FlagInfo[] | undefined) => {
    if (selections && selections.length > 0) {
      onValueChanged(selections[0].Hun);
    }
  }, [onValueChanged]);
  if(ref.current && !value) {
    ref.current.clear();
  }
  return (
      <Typeahead
        ref={ref}
        caseSensitive={false}
        ignoreDiacritics={true}
        options={flags}
        placeholder="Country name"
        highlightOnlyResult
        onChange={changeHandler}
        labelKey={labelFormatter}
        align="left"
        maxResults={8}
        flip
      />
  );
};

function labelFormatter(option: FlagInfo) {
  return option.Hun;
}
