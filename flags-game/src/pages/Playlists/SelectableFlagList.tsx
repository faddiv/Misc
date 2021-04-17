import { FunctionComponent } from "react";
import { Row } from "react-bootstrap";
import { FlagChecked } from "../../flagsService/playList";
import { SelectableFlag } from "./SelectableFlag";

interface SelectableFlagListProps {
  flags: FlagChecked[];
  onFlagChecked: (flag: FlagChecked) => void;
}

export const SelectableFlagList: FunctionComponent<SelectableFlagListProps> = ({ flags, onFlagChecked }) => {
  if (flags === null) {
    return null;
  }
  return (
    <Row>
      {flags.map(flag => <SelectableFlag key={flag.Pic} flag={flag} onFlagChecked={onFlagChecked} />)}
    </Row>
  );
};
