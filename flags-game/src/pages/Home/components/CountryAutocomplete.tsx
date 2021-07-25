import { FunctionComponent, useCallback, useEffect, useRef } from "react";
import { Typeahead } from "react-bootstrap-typeahead";
import { FlagInfo } from "../../../flagsService/flagList";

interface CountryAutocompleteProps {
  flags: FlagInfo[],
  value: string;
  onValueChanged: (newValue: string) => void;
}

export const CountryAutocomplete: FunctionComponent<CountryAutocompleteProps> = ({ flags, value, onValueChanged }) => {
  const ref = useRef<Typeahead<FlagInfo>>(null);
  const changeHandler = useCallback((selections: FlagInfo[] | undefined) => {
    if (selections && selections.length > 0) {
      onValueChanged(selections[0].Hun);
    }
  }, [onValueChanged]);
  useEffect(() => {
    if(ref.current && !value) {
      ref.current.clear();
    }
  }, [value]);
  return (
      <Typeahead
        id="country-name"
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
